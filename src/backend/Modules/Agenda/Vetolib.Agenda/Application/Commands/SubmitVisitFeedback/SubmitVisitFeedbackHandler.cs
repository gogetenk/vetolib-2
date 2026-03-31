using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Application.Commands.SubmitVisitFeedback;

internal class SubmitVisitFeedbackHandler : IRequestHandler<SubmitVisitFeedbackCommand, Result<VisitFeedbackDto>>
{
    private readonly AgendaDbContext _context;
    private readonly IClinicContext _clinicContext;

    public SubmitVisitFeedbackHandler(AgendaDbContext context, IClinicContext clinicContext)
    {
        _context = context;
        _clinicContext = clinicContext;
    }

    public async Task<Result<VisitFeedbackDto>> Handle(SubmitVisitFeedbackCommand cmd, CancellationToken ct)
    {
        // Verify the appointment exists and belongs to the current clinic
        var appointment = await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == cmd.AppointmentId, ct);

        if (appointment is null)
            return Result<VisitFeedbackDto>.NotFound("APPOINTMENT_NOT_FOUND:Appointment not found");

        // Only allow feedback on completed appointments
        if (appointment.Status != AppointmentStatus.Completed)
            return Result<VisitFeedbackDto>.Error("APPOINTMENT_NOT_COMPLETED:Feedback can only be submitted for completed appointments");

        // Check if feedback already exists for this appointment
        var existingFeedback = await _context.VisitFeedbacks
            .AsNoTracking()
            .AnyAsync(f => f.AppointmentId == cmd.AppointmentId, ct);

        if (existingFeedback)
            return Result<VisitFeedbackDto>.Error("FEEDBACK_ALREADY_EXISTS:Feedback has already been submitted for this appointment");

        // Create feedback via domain factory
        var feedbackResult = VisitFeedback.Create(
            _clinicContext.ClinicId,
            cmd.AppointmentId,
            cmd.Rating,
            cmd.Comment,
            cmd.IsPublic);

        if (!feedbackResult.IsSuccess)
            return Result<VisitFeedbackDto>.Invalid(feedbackResult.ValidationErrors.ToList());

        _context.VisitFeedbacks.Add(feedbackResult.Value);
        await _context.SaveChangesAsync(ct);

        return Result<VisitFeedbackDto>.Success(feedbackResult.Value.ToDto());
    }
}
