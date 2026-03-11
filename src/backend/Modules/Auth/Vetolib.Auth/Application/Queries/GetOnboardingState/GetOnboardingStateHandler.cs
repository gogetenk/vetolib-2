using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Auth.Application.Queries.GetOnboardingState;

internal sealed record OnboardingDataSnapshot(
    OnboardingState State,
    int PatientCount,
    int AppointmentCount,
    int InvoiceCount
);

internal class GetOnboardingStateHandler : IRequestHandler<GetOnboardingStateQuery, Result<OnboardingStateDto>>
{
    private readonly AuthDbContext _context;
    private readonly ISender _sender;

    public GetOnboardingStateHandler(AuthDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<Result<OnboardingStateDto>> Handle(GetOnboardingStateQuery query, CancellationToken ct)
    {
        var snapshotResult = await LoadSnapshotAsync(query.UserId, ct);
        if (!snapshotResult.IsSuccess)
            return snapshotResult.Map(_ => (OnboardingStateDto)null!);

        var snapshot = snapshotResult.Value;
        ApplyAutoCompletions(snapshot);
        await _context.SaveChangesAsync(ct);

        return Result<OnboardingStateDto>.Success(ComputeState(snapshot));
    }

    private async Task<Result<OnboardingDataSnapshot>> LoadSnapshotAsync(Guid userId, CancellationToken ct)
    {
        var state = await _context.OnboardingStates
            .FirstOrDefaultAsync(o => o.UserId == userId, ct);

        if (state is null)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user is null)
                return Result<OnboardingDataSnapshot>.NotFound("User not found");

            var createResult = OnboardingState.Create(userId, user.Role.ToString(), user.ClinicId);
            if (!createResult.IsSuccess)
                return createResult.Map(_ => (OnboardingDataSnapshot)null!);

            state = createResult.Value;
            _context.OnboardingStates.Add(state);
            await _context.SaveChangesAsync(ct);
        }

        var roleSteps = OnboardingSteps.GetStepsForRole(state.Role);

        var patientCountResult = roleSteps.Contains(OnboardingSteps.AddFirstPatient)
            ? await _sender.Send(new GetPatientCountQuery(), ct)
            : Result<int>.Success(0);
        var patientCount = patientCountResult.IsSuccess ? patientCountResult.Value : 0;

        var appointmentCountResult = roleSteps.Contains(OnboardingSteps.BookFirstAppointment)
            ? await _sender.Send(new GetAppointmentCountQuery(), ct)
            : Result<int>.Success(0);
        var appointmentCount = appointmentCountResult.IsSuccess ? appointmentCountResult.Value : 0;

        var invoiceCountResult = roleSteps.Contains(OnboardingSteps.CreateFirstInvoice)
            ? await _sender.Send(new GetInvoiceCountQuery(), ct)
            : Result<int>.Success(0);
        var invoiceCount = invoiceCountResult.IsSuccess ? invoiceCountResult.Value : 0;

        return Result<OnboardingDataSnapshot>.Success(
            new OnboardingDataSnapshot(state, patientCount, appointmentCount, invoiceCount));
    }

    private static void ApplyAutoCompletions(OnboardingDataSnapshot snapshot)
    {
        var (state, patientCount, appointmentCount, invoiceCount) = snapshot;
        var roleSteps = OnboardingSteps.GetStepsForRole(state.Role);

        if (roleSteps.Contains(OnboardingSteps.AddFirstPatient)
            && !state.CompletedSteps.Contains(OnboardingSteps.AddFirstPatient)
            && patientCount > 0)
            state.CompleteStep(OnboardingSteps.AddFirstPatient);

        if (roleSteps.Contains(OnboardingSteps.BookFirstAppointment)
            && !state.CompletedSteps.Contains(OnboardingSteps.BookFirstAppointment)
            && appointmentCount > 0)
            state.CompleteStep(OnboardingSteps.BookFirstAppointment);

        if (roleSteps.Contains(OnboardingSteps.CreateFirstInvoice)
            && !state.CompletedSteps.Contains(OnboardingSteps.CreateFirstInvoice)
            && invoiceCount > 0)
            state.CompleteStep(OnboardingSteps.CreateFirstInvoice);

        if (!state.CompletedAt.HasValue && state.IsFullyCompleted(roleSteps))
            state.MarkCompleted();
    }

    private static OnboardingStateDto ComputeState(OnboardingDataSnapshot snapshot)
    {
        var state = snapshot.State;
        var roleSteps = OnboardingSteps.GetStepsForRole(state.Role);

        var stepDtos = roleSteps.Select(stepId => new OnboardingStepDto(
            stepId,
            OnboardingSteps.GetLabelForStep(stepId),
            state.CompletedSteps.Contains(stepId),
            OnboardingSteps.GetLinkForStep(stepId)
        )).ToList();

        var completed = stepDtos.Count(s => s.IsCompleted);
        var total = stepDtos.Count;
        var isCompleted = state.CompletedAt.HasValue || (total > 0 && completed == total);

        return new OnboardingStateDto(
            WelcomeBannerVisible: !state.WelcomeBannerDismissed,
            ChecklistVisible: !state.ChecklistDismissed,
            Steps: stepDtos,
            Progress: new OnboardingProgressDto(completed, total),
            IsCompleted: isCompleted
        );
    }
}
