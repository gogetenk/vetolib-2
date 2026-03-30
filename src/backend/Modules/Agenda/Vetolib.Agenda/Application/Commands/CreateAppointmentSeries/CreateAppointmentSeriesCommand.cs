using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.CreateAppointmentSeries;

internal record CreateAppointmentSeriesCommand(
    Guid ClinicId,
    Guid VeterinarianId,
    string VeterinarianName,
    Guid AnimalId,
    string AnimalName,
    string OwnerName,
    string? OwnerEmail,
    DateOnly StartDate,
    TimeOnly StartTime,
    int DurationMinutes,
    string? Reason,
    RecurrenceFrequency Frequency,
    int Count) : IRequest<Result<List<AppointmentDto>>>;
