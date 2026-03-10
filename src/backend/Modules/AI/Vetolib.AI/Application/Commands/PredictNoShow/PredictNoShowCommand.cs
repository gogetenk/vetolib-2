using Ardalis.Result;
using MediatR;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Commands.PredictNoShow;

internal record PredictNoShowCommand(Guid AppointmentId, Guid UserId) : IRequest<Result<NoShowPredictionDto>>;
