using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.SuggestSlot;

internal record SuggestSlotQuery(
    string ConsultationType,
    DateOnly PreferredDate,
    TimeOnly PreferredTime,
    Guid? PreferredVeterinarianId,
    int? DurationMinutes) : IRequest<Result<SlotSuggestionsResponse>>;
