using Microsoft.Extensions.Options;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Services;

internal record VetScheduleInfo(
    Guid VeterinarianId,
    string VeterinarianName,
    IReadOnlyList<Appointment> AppointmentsOnDate);

/// <summary>
/// Scores candidate slots on a 0-100 scale across 5 weighted criteria.
/// Weights are configurable via appsettings.json Agenda:SlotScoring.
/// </summary>
internal class SlotScoringService
{
    private readonly SlotScoringOptions _weights;
    private readonly AgendaWorkingHoursOptions _hours;

    public SlotScoringService(IOptions<AgendaOptions> options)
    {
        _weights = options.Value.SlotScoring;
        _hours = options.Value.WorkingHours;
    }

    /// <summary>
    /// Generates and scores candidate slots for a given vet on the requested date.
    /// Returns a list of <see cref="ScoredSlot"/> sorted by score descending.
    /// </summary>
    public IReadOnlyList<ScoredSlot> ScoreSlots(
        VetScheduleInfo vetInfo,
        IReadOnlyList<VetScheduleInfo> allVets,
        DateOnly date,
        TimeOnly preferredTime,
        string consultationType,
        int durationMinutes)
    {
        var slots = new List<ScoredSlot>();

        var workStart = new TimeOnly(_hours.StartHour, 0);
        var workEnd = new TimeOnly(_hours.EndHour, 0);

        // Generate candidate slot times every 15 minutes within working hours
        var current = workStart;
        while (current.AddMinutes(durationMinutes) <= workEnd)
        {
            var slotEnd = current.AddMinutes(durationMinutes);

            // Check if slot overlaps with existing appointments
            var conflicts = vetInfo.AppointmentsOnDate
                .Where(a => a.StartTime < slotEnd && a.EndTime > current)
                .ToList();

            if (conflicts.Count == 0)
            {
                var score = ComputeScore(
                    current,
                    slotEnd,
                    vetInfo,
                    allVets,
                    preferredTime,
                    consultationType,
                    durationMinutes);

                slots.Add(new ScoredSlot(
                    vetInfo.VeterinarianId,
                    vetInfo.VeterinarianName,
                    date,
                    current,
                    durationMinutes,
                    score.TotalScore,
                    score.Reasoning));
            }

            current = current.AddMinutes(15);
        }

        return slots.OrderByDescending(s => s.Score).ToList();
    }

    private ScoreBreakdown ComputeScore(
        TimeOnly slotStart,
        TimeOnly slotEnd,
        VetScheduleInfo vetInfo,
        IReadOnlyList<VetScheduleInfo> allVets,
        TimeOnly preferredTime,
        string consultationType,
        int durationMinutes)
    {
        var reasons = new List<string>();

        // ─── 1. Gap minimization (30%) ───────────────────────────────────────────
        // Score is highest when the slot is immediately adjacent to an existing appointment.
        var gapScore = ComputeGapScore(slotStart, slotEnd, vetInfo.AppointmentsOnDate, reasons);

        // ─── 2. Load balancing (25%) ─────────────────────────────────────────────
        var loadScore = ComputeLoadScore(vetInfo, allVets, reasons);

        // ─── 3. Type grouping (20%) ──────────────────────────────────────────────
        var typeScore = ComputeTypeGroupingScore(consultationType, vetInfo.AppointmentsOnDate, slotStart, reasons);

        // ─── 4. Vet preference (15%) — simplified: morning preference if > 12:00 half day ─
        var vetPrefScore = ComputeVetPreferenceScore(slotStart, reasons);

        // ─── 5. Client proximity (10%) ───────────────────────────────────────────
        var proximityScore = ComputeProximityScore(slotStart, preferredTime, reasons);

        var total = (int)Math.Round(
            gapScore * _weights.GapMinimizationWeight * 100 +
            loadScore * _weights.LoadBalancingWeight * 100 +
            typeScore * _weights.TypeGroupingWeight * 100 +
            vetPrefScore * _weights.VetPreferenceWeight * 100 +
            proximityScore * _weights.ClientProximityWeight * 100);

        total = Math.Clamp(total, 0, 100);

        return new ScoreBreakdown(total, string.Join("; ", reasons.Where(r => !string.IsNullOrEmpty(r))));
    }

    // ─── Criterion implementations ────────────────────────────────────────────

    private static double ComputeGapScore(
        TimeOnly slotStart,
        TimeOnly slotEnd,
        IReadOnlyList<Appointment> appointments,
        List<string> reasons)
    {
        if (!appointments.Any())
        {
            reasons.Add("Aucun rendez-vous existant ce jour");
            return 0.5; // neutral score when calendar is empty
        }

        // Find the gap between this slot and adjacent appointments
        var prevEnd = appointments
            .Where(a => a.EndTime <= slotStart)
            .OrderByDescending(a => a.EndTime)
            .Select(a => (TimeOnly?)a.EndTime)
            .FirstOrDefault();

        var nextStart = appointments
            .Where(a => a.StartTime >= slotEnd)
            .OrderBy(a => a.StartTime)
            .Select(a => (TimeOnly?)a.StartTime)
            .FirstOrDefault();

        double score;
        if (prevEnd.HasValue && prevEnd.Value == slotStart)
        {
            reasons.Add("Creneau adjacent au precedent (pas de gap)");
            score = 1.0;
        }
        else if (nextStart.HasValue && nextStart.Value == slotEnd)
        {
            reasons.Add("Creneau adjacent au suivant (pas de gap)");
            score = 1.0;
        }
        else
        {
            // Penalize proportionally to gap size (max gap considered = 120 min)
            var beforeGap = prevEnd.HasValue ? (slotStart - prevEnd.Value).TotalMinutes : 120;
            var afterGap = nextStart.HasValue ? (nextStart.Value - slotEnd).TotalMinutes : 120;
            var minGap = Math.Min(beforeGap, afterGap);
            score = Math.Max(0, 1.0 - minGap / 120.0);

            if (minGap > 0)
                reasons.Add($"Gap de {(int)minGap} min avec le rendez-vous adjacent");
        }

        return score;
    }

    private static double ComputeLoadScore(
        VetScheduleInfo vetInfo,
        IReadOnlyList<VetScheduleInfo> allVets,
        List<string> reasons)
    {
        if (allVets.Count <= 1)
            return 0.5;

        var averageLoad = allVets.Average(v => v.AppointmentsOnDate.Count);
        var thisLoad = vetInfo.AppointmentsOnDate.Count;

        if (thisLoad < averageLoad)
        {
            reasons.Add("Charge inferieure a la moyenne (equilibrage favorise)");
            return 1.0;
        }
        else if (thisLoad > averageLoad)
        {
            var overload = thisLoad - averageLoad;
            return Math.Max(0, 1.0 - overload / 5.0); // 5 extra appointments = score 0
        }

        return 0.5;
    }

    private static double ComputeTypeGroupingScore(
        string consultationType,
        IReadOnlyList<Appointment> appointments,
        TimeOnly slotStart,
        List<string> reasons)
    {
        if (!appointments.Any())
            return 0.5;

        // Determine session: morning = before 13:00, afternoon = 13:00+
        var isMorning = slotStart < new TimeOnly(13, 0);

        var sameSessionAppointments = appointments
            .Where(a => (a.StartTime < new TimeOnly(13, 0)) == isMorning)
            .ToList();

        var sameTypeInSession = sameSessionAppointments
            .Where(a => a.Reason != null && a.Reason.ToLower().StartsWith(consultationType.ToLower()))
            .Count();

        if (sameTypeInSession > 0)
        {
            reasons.Add($"Regroupement avec {sameTypeInSession} consultation(s) du meme type ({consultationType})");
            return 1.0;
        }

        return 0.3;
    }

    private static double ComputeVetPreferenceScore(TimeOnly slotStart, List<string> reasons)
    {
        // Simple heuristic: prefer morning slots (before 13:00) as default vet preference
        if (slotStart < new TimeOnly(13, 0))
        {
            reasons.Add("Creneau matinal (preference horaire vet)");
            return 1.0;
        }

        return 0.5;
    }

    private static double ComputeProximityScore(TimeOnly slotStart, TimeOnly preferredTime, List<string> reasons)
    {
        var diffMinutes = Math.Abs((slotStart - preferredTime).TotalMinutes);

        if (diffMinutes == 0)
        {
            reasons.Add("Heure exacte demandee par le client");
            return 1.0;
        }

        // Score decays over 4 hours (240 min)
        var score = Math.Max(0, 1.0 - diffMinutes / 240.0);

        if (diffMinutes <= 30)
            reasons.Add($"Proche de l'heure demandee ({(int)diffMinutes} min d'ecart)");
        else if (diffMinutes <= 120)
            reasons.Add($"Ecart de {(int)diffMinutes} min avec l'heure demandee");

        return score;
    }

    private record ScoreBreakdown(int TotalScore, string Reasoning);
}

internal record ScoredSlot(
    Guid VeterinarianId,
    string VeterinarianName,
    DateOnly Date,
    TimeOnly StartTime,
    int DurationMinutes,
    int Score,
    string Reasoning);
