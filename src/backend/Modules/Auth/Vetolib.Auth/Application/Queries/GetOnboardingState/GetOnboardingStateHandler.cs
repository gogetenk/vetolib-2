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
        // Load or lazy-init the onboarding state
        var state = await _context.OnboardingStates
            .FirstOrDefaultAsync(o => o.UserId == query.UserId, ct);

        if (state is null)
        {
            // Lazy init: load user to get role
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == query.UserId, ct);

            if (user is null)
                return Result<OnboardingStateDto>.NotFound("User not found");

            var createResult = OnboardingState.Create(query.UserId, user.Role.ToString(), user.ClinicId);
            if (!createResult.IsSuccess)
                return createResult.Map(_ => (OnboardingStateDto)null!);

            state = createResult.Value;
            _context.OnboardingStates.Add(state);
            await _context.SaveChangesAsync(ct);
        }

        // Auto-completion for data-driven steps (Admin role only for now)
        await RunAutoCompletionAsync(state, ct);

        // Save any auto-completed steps
        await _context.SaveChangesAsync(ct);

        return Result<OnboardingStateDto>.Success(BuildDto(state));
    }

    private async Task RunAutoCompletionAsync(OnboardingState state, CancellationToken ct)
    {
        var roleSteps = OnboardingSteps.GetStepsForRole(state.Role);

        // add_first_patient — auto-complete if clinic has at least 1 patient
        if (roleSteps.Contains(OnboardingSteps.AddFirstPatient)
            && !state.CompletedSteps.Contains(OnboardingSteps.AddFirstPatient))
        {
            var countResult = await _sender.Send(new GetPatientCountQuery(), ct);
            if (countResult.IsSuccess && countResult.Value > 0)
                state.CompleteStep(OnboardingSteps.AddFirstPatient);
        }

        // book_first_appointment — auto-complete if clinic has at least 1 appointment
        if (roleSteps.Contains(OnboardingSteps.BookFirstAppointment)
            && !state.CompletedSteps.Contains(OnboardingSteps.BookFirstAppointment))
        {
            var countResult = await _sender.Send(new GetAppointmentCountQuery(), ct);
            if (countResult.IsSuccess && countResult.Value > 0)
                state.CompleteStep(OnboardingSteps.BookFirstAppointment);
        }

        // create_first_invoice — auto-complete if clinic has at least 1 invoice
        if (roleSteps.Contains(OnboardingSteps.CreateFirstInvoice)
            && !state.CompletedSteps.Contains(OnboardingSteps.CreateFirstInvoice))
        {
            var countResult = await _sender.Send(new GetInvoiceCountQuery(), ct);
            if (countResult.IsSuccess && countResult.Value > 0)
                state.CompleteStep(OnboardingSteps.CreateFirstInvoice);
        }

        // Check if all steps completed → mark as done
        if (!state.CompletedAt.HasValue && state.IsFullyCompleted(roleSteps))
        {
            state.MarkCompleted();
        }
    }

    private static OnboardingStateDto BuildDto(OnboardingState state)
    {
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
