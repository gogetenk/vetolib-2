using Ardalis.Result;
using MediatR;

namespace Vetolib.Messaging.Application.Commands.SendWhatsAppTest;

internal record SendWhatsAppTestCommand(
    string RecipientPhone,
    string TemplateName) : IRequest<Result>;
