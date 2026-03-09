using Ardalis.Result;
using MediatR;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Commands.PredictNoShowBatch;

internal record PredictNoShowBatchCommand(DateOnly Date) : IRequest<Result<List<NoShowPredictionDto>>>;
