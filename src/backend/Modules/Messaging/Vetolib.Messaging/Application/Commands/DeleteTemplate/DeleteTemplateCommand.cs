using Ardalis.Result;
using MediatR;

namespace Vetolib.Messaging.Application.Commands.DeleteTemplate;

internal record DeleteTemplateCommand(Guid Id) : IRequest<Result>;
