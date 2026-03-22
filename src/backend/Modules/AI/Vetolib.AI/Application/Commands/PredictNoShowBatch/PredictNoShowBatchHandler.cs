using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Options;
using Vetolib.Agenda.Contracts;
using Vetolib.AI.Application.Commands.PredictNoShow;
using Vetolib.AI.Application.Services;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Commands.PredictNoShowBatch;

internal class PredictNoShowBatchHandler : IRequestHandler<PredictNoShowBatchCommand, Result<List<NoShowPredictionDto>>>
{
    private readonly IAppointmentReader _reader;
    private readonly INoShowPredictionService _predictionService;
    private readonly AIOptions _options;

    public PredictNoShowBatchHandler(
        IAppointmentReader reader,
        INoShowPredictionService predictionService,
        IOptions<AIOptions> options)
    {
        _reader = reader;
        _predictionService = predictionService;
        _options = options.Value;
    }

    public async Task<Result<List<NoShowPredictionDto>>> Handle(
        PredictNoShowBatchCommand command,
        CancellationToken ct)
    {
        var totalCompleted = await _reader.GetCompletedAppointmentCountAsync(ct);
        if (totalCompleted < _options.MinimumHistoricalAppointments)
            return Result<List<NoShowPredictionDto>>.Error("INSUFFICIENT_DATA");

        var appointmentFeatures = await _reader.GetAppointmentsByDateAsync(command.Date, ct);

        var predictions = new List<NoShowPredictionDto>(appointmentFeatures.Count);

        foreach (var features in appointmentFeatures)
        {
            var input = PredictNoShowHandler.BuildInput(features);
            var result = _predictionService.Predict(input, features.AppointmentId);

            if (result.IsSuccess)
                predictions.Add(result.Value);
            // Skip appointments where prediction fails (e.g. model not loaded)
            // rather than failing the entire batch
        }

        return Result<List<NoShowPredictionDto>>.Success(predictions);
    }
}
