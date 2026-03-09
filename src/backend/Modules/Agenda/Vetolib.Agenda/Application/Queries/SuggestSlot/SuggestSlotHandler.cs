using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Application.Services;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;

namespace Vetolib.Agenda.Application.Queries.SuggestSlot;

internal class SuggestSlotHandler : IRequestHandler<SuggestSlotQuery, Result<SlotSuggestionsResponse>>
{
    private readonly AgendaDbContext _context;
    private readonly SlotScoringService _scoringService;
    private readonly DurationEstimator _durationEstimator;

    public SuggestSlotHandler(
        AgendaDbContext context,
        SlotScoringService scoringService,
        DurationEstimator durationEstimator)
    {
        _context = context;
        _scoringService = scoringService;
        _durationEstimator = durationEstimator;
    }

    public async Task<Result<SlotSuggestionsResponse>> Handle(
        SuggestSlotQuery query,
        CancellationToken ct)
    {
        // Load all appointments for the requested date
        var allAppointments = await _context.Appointments
            .Where(a => a.Date == query.PreferredDate &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow)
            .ToListAsync(ct);

        // Determine which vets to consider
        var vetIds = query.PreferredVeterinarianId.HasValue
            ? new[] { query.PreferredVeterinarianId.Value }
            : allAppointments
                .Select(a => a.VeterinarianId)
                .Distinct()
                .ToArray();

        // Build vet schedule info list
        var vetSchedules = vetIds
            .Select(vetId =>
            {
                var appts = allAppointments.Where(a => a.VeterinarianId == vetId).ToList();
                var vetName = appts.FirstOrDefault()?.VeterinarianName ?? $"Vet {vetId:N}";
                return new VetScheduleInfo(vetId, vetName, appts);
            })
            .ToList();

        // If no appointments yet but a preferred vet was specified, include that vet with an empty schedule
        if (query.PreferredVeterinarianId.HasValue && !vetSchedules.Any())
        {
            vetSchedules.Add(new VetScheduleInfo(
                query.PreferredVeterinarianId.Value,
                $"Vet {query.PreferredVeterinarianId.Value:N}",
                new List<Appointment>()));
        }

        // Score slots for each vet
        var allScoredSlots = new List<ScoredSlot>();

        foreach (var vetSchedule in vetSchedules)
        {
            // Estimate duration for this vet
            var durationResult = await _durationEstimator.EstimateAsync(
                vetSchedule.VeterinarianId,
                query.ConsultationType,
                ct);

            var duration = query.DurationMinutes
                ?? (durationResult.IsSuccess ? durationResult.Value : 30);

            var slots = _scoringService.ScoreSlots(
                vetSchedule,
                vetSchedules,
                query.PreferredDate,
                query.PreferredTime,
                query.ConsultationType,
                duration);

            allScoredSlots.AddRange(slots);
        }

        // Return top 3 by score
        var top3 = allScoredSlots
            .OrderByDescending(s => s.Score)
            .Take(3)
            .Select(s => new SlotSuggestionDto(
                s.VeterinarianId,
                s.VeterinarianName,
                s.Date,
                s.StartTime,
                s.DurationMinutes,
                s.Score,
                s.Reasoning))
            .ToList();

        return Result<SlotSuggestionsResponse>.Success(new SlotSuggestionsResponse(top3));
    }
}
