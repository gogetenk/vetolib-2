using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.AI.Contracts;
using Vetolib.AI.Infrastructure;

namespace Vetolib.AI.Application.Commands.ConvertAlertToAppointment;

internal class ConvertAlertToAppointmentHandler : IRequestHandler<ConvertAlertToAppointmentCommand, Result<AppointmentPreFillDto>>
{
    /// <summary>
    /// Placeholder appointment ID used when converting an alert to a pre-filled appointment.
    /// The actual appointment ID will be assigned when the appointment is created.
    /// </summary>
    internal static readonly Guid PlaceholderAppointmentId = new("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");

    private readonly AIDbContext _context;

    public ConvertAlertToAppointmentHandler(AIDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentPreFillDto>> Handle(
        ConvertAlertToAppointmentCommand cmd,
        CancellationToken ct)
    {
        var alert = await _context.HealthAlerts
            .FirstOrDefaultAsync(a => a.Id == cmd.AlertId, ct);

        if (alert is null)
            return Result<AppointmentPreFillDto>.NotFound($"Health alert {cmd.AlertId} not found.");

        // Use a placeholder appointment ID — the actual appointment
        // will be created by the frontend using the pre-fill data
        var result = alert.MarkScheduled(PlaceholderAppointmentId);
        if (!result.IsSuccess)
            return Result<AppointmentPreFillDto>.Error(string.Join("; ", result.Errors));

        await _context.SaveChangesAsync(ct);

        var preFill = new AppointmentPreFillDto(
            PatientId: alert.PatientId,
            PatientName: string.Empty, // Patient name is not stored in HealthAlert — frontend resolves it
            SuggestedNotes: alert.RecommendedAction ?? alert.Description,
            AlertTitle: alert.Title);

        return Result<AppointmentPreFillDto>.Success(preFill);
    }
}
