using Ardalis.Result;
using MediatR;

namespace Vetolib.AI.Application.Commands.AcceptTriage;

internal record AcceptTriageCommand(Guid TriageId) : IRequest<Result>;
