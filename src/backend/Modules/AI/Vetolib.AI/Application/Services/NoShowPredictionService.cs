using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Vetolib.AI.Application.ML;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Services;

internal interface INoShowPredictionService
{
    Result<NoShowPredictionDto> Predict(NoShowInput input, Guid appointmentId);
}

internal sealed class NoShowPredictionService : INoShowPredictionService
{
    private readonly PredictionEnginePool<NoShowInput, NoShowOutput>? _pool;
    private readonly ILogger<NoShowPredictionService> _logger;

    public NoShowPredictionService(
        ILogger<NoShowPredictionService> logger,
        PredictionEnginePool<NoShowInput, NoShowOutput>? pool = null)
    {
        _logger = logger;
        _pool = pool;
    }

    public Result<NoShowPredictionDto> Predict(NoShowInput input, Guid appointmentId)
    {
        if (_pool is null)
        {
            _logger.LogWarning("No-show prediction requested but ML model is not loaded.");
            return Result<NoShowPredictionDto>.Error("ML_MODEL_NOT_LOADED");
        }

        NoShowOutput output;
        try
        {
            output = _pool.Predict(input);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ML prediction engine threw an exception for appointment {AppointmentId}.", appointmentId);
            return Result<NoShowPredictionDto>.Error("ML_PREDICTION_FAILED");
        }

        var riskLevel = output.Probability switch
        {
            > 0.5f => "High",
            > 0.3f => "Medium",
            _ => "Low"
        };

        var topFactors = BuildTopFactors(input);
        var suggestions = BuildSuggestions(riskLevel, input);

        var dto = new NoShowPredictionDto(
            AppointmentId: appointmentId,
            NoShowProbability: output.Probability,
            RiskLevel: riskLevel,
            TopFactors: topFactors,
            Suggestions: suggestions);

        return Result<NoShowPredictionDto>.Success(dto);
    }

    private static List<string> BuildTopFactors(NoShowInput input)
    {
        var factors = new List<(string Factor, float Weight)>();

        if (input.HistoricalNoShowRate > 0.3f)
            factors.Add(("high_historical_noshow_rate", input.HistoricalNoShowRate));

        if (input.DayOfWeek is 5 or 6) // Friday=5, Saturday=6 — UAE weekend
            factors.Add(("weekend_appointment", 0.4f));

        if (input.HourOfDay is < 9f or > 17f)
            factors.Add(("off_peak_hour", 0.3f));

        if (input.DaysSinceLastVisit == 0)
            factors.Add(("new_patient", 0.35f));
        else if (input.DaysSinceLastVisit > 365)
            factors.Add(("long_absence", 0.3f));

        if (!input.WasReminderSent)
            factors.Add(("no_reminder_sent", 0.45f));

        if (input.OwnerTotalAppointments == 0)
            factors.Add(("first_appointment", 0.3f));

        if (input.LeadTimeDays > 14)
            factors.Add(("long_lead_time", 0.25f));

        // Return top 3 factors by weight
        return factors
            .OrderByDescending(f => f.Weight)
            .Take(3)
            .Select(f => f.Factor)
            .ToList();
    }

    private static List<string> BuildSuggestions(string riskLevel, NoShowInput input)
    {
        var suggestions = new List<string>();

        if (riskLevel == "High")
        {
            suggestions.Add("send_extra_reminder");

            if (input.LeadTimeDays > 7)
                suggestions.Add("confirm_appointment_48h_before");

            suggestions.Add("consider_overbooking_slot");
        }
        else if (riskLevel == "Medium")
        {
            if (!input.WasReminderSent)
                suggestions.Add("send_reminder");

            suggestions.Add("confirm_appointment_24h_before");
        }

        return suggestions;
    }
}
