using Ardalis.Result;
using Vetolib.AI.Application.ML;
using Vetolib.AI.Application.Services;
using Vetolib.AI.Contracts;

namespace Vetolib.Tests.Acceptance.Support;

/// <summary>
/// Test double for INoShowPredictionService.
/// Returns deterministic predictions based on input features so that
/// acceptance tests do not depend on a trained ML model file.
/// Risk level logic mirrors NoShowPredictionService thresholds:
///   Probability > 0.5 → High, > 0.3 → Medium, else Low.
/// </summary>
internal sealed class FakeNoShowPredictionService : INoShowPredictionService
{
    public Result<NoShowPredictionDto> Predict(NoShowInput input, Guid appointmentId)
    {
        // Derive a deterministic probability from the input features
        float probability;

        if (input.HistoricalNoShowRate > 0.35f)
        {
            // High historical no-show rate → High risk
            probability = 0.65f;
        }
        else if (input.DayOfWeek is 5 or 6) // Friday/Saturday — UAE weekend
        {
            // Weekend appointment → Medium/High depending on other factors
            probability = 0.45f;
        }
        else
        {
            // Default → Medium risk
            probability = 0.35f;
        }

        var riskLevel = probability switch
        {
            > 0.5f => "High",
            > 0.3f => "Medium",
            _ => "Low"
        };

        var topFactors = BuildTopFactors(input);
        var suggestions = BuildSuggestions(riskLevel, input);

        return Result<NoShowPredictionDto>.Success(new NoShowPredictionDto(
            AppointmentId: appointmentId,
            NoShowProbability: probability,
            RiskLevel: riskLevel,
            TopFactors: topFactors,
            Suggestions: suggestions));
    }

    private static List<string> BuildTopFactors(NoShowInput input)
    {
        var factors = new List<string>();

        if (input.HistoricalNoShowRate > 0.3f)
            factors.Add("high_historical_noshow_rate");

        if (input.DayOfWeek is 5 or 6)
            factors.Add("weekend_appointment");

        if (!input.WasReminderSent)
            factors.Add("no_reminder_sent");

        if (input.LeadTimeDays > 14)
            factors.Add("long_lead_time");

        if (factors.Count == 0)
            factors.Add("appointment_type");

        return factors.Take(3).ToList();
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
