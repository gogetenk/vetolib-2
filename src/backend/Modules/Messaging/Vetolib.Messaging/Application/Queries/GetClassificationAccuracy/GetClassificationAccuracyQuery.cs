using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.GetClassificationAccuracy;

internal record GetClassificationAccuracyQuery : IRequest<Result<ClassificationAccuracyDto>>;
