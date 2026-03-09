using Ardalis.Result;
using MediatR;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Commands.OverrideTriage;

internal record OverrideTriageCommand(Guid TriageId, AISeverity NewSeverity) : IRequest<Result>;
