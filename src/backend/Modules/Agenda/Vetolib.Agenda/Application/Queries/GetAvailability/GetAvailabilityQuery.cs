using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.GetAvailability;

internal record GetAvailabilityQuery(
    Guid VeterinarianId,
    DateOnly Date,
    int DurationMinutes) : IRequest<Result<IReadOnlyList<AvailabilitySlotDto>>>;
