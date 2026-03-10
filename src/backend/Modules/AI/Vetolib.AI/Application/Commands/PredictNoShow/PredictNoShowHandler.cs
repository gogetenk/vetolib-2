using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;
using Vetolib.AI.Application.ML;
using Vetolib.AI.Application.Services;
using Vetolib.AI.Contracts;
using Vetolib.Preferences.Contracts;

namespace Vetolib.AI.Application.Commands.PredictNoShow;

internal class PredictNoShowHandler : IRequestHandler<PredictNoShowCommand, Result<NoShowPredictionDto>>
{
    private const int MinimumHistoricalAppointments = 50;

    private readonly IAppointmentReader _reader;
    private readonly INoShowPredictionService _predictionService;
    private readonly IPreferenceChecker _preferenceChecker;

    public PredictNoShowHandler(
        IAppointmentReader reader,
        INoShowPredictionService predictionService,
        IPreferenceChecker preferenceChecker)
    {
        _reader = reader;
        _predictionService = predictionService;
        _preferenceChecker = preferenceChecker;
    }

    public async Task<Result<NoShowPredictionDto>> Handle(
        PredictNoShowCommand command,
        CancellationToken ct)
    {
        // Check AINoShow preference for this user (defaults to true).
        if (command.UserId != Guid.Empty)
        {
            var prefResult = await _preferenceChecker.IsTrueAsync(command.UserId, PreferenceKey.AINoShow, ct);
            if (prefResult.IsSuccess && !prefResult.Value)
                return Result<NoShowPredictionDto>.Error("AI_NOSHOW_DISABLED");
        }

        // Cold start check — not enough historical data to make reliable predictions
        var totalCompleted = await _reader.GetCompletedAppointmentCountAsync(ct);
        if (totalCompleted < MinimumHistoricalAppointments)
            return Result<NoShowPredictionDto>.Error("INSUFFICIENT_DATA");

        var features = await _reader.GetFeaturesForPredictionAsync(command.AppointmentId, ct);
        if (features is null)
            return Result<NoShowPredictionDto>.NotFound();

        var input = BuildInput(features);
        return _predictionService.Predict(input, command.AppointmentId);
    }

    internal static NoShowInput BuildInput(AppointmentFeaturesDto features)
    {
        var total = features.OwnerTotalAppointments;
        var noShowRate = total > 0
            ? (float)features.OwnerNoShowCount / total
            : 0f;

        // DayOfWeek: Sunday=0 (DayOfWeek.Sunday=0 in .NET), matches UAE week
        var dateTime = features.Date.ToDateTime(features.StartTime);
        var dayOfWeek = (int)dateTime.DayOfWeek;
        var hourOfDay = features.StartTime.Hour + features.StartTime.Minute / 60f;

        return new NoShowInput
        {
            HistoricalNoShowRate = noShowRate,
            DayOfWeek = dayOfWeek,
            HourOfDay = hourOfDay,
            DaysSinceLastVisit = features.DaysSinceLastVisit,
            AppointmentType = NormalizeType(features.ConsultationType),
            OwnerTotalAppointments = features.OwnerTotalAppointments,
            WasReminderSent = features.WasReminderSent,
            LeadTimeDays = features.LeadTimeDays
        };
    }

    private static string NormalizeType(string? reason) =>
        string.IsNullOrWhiteSpace(reason) ? "general" : reason.ToLowerInvariant().Trim();
}
