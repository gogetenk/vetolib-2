using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.UpdateWhatsAppConfig;

internal record UpdateWhatsAppConfigCommand(
    string WabaId,
    string PhoneNumberId,
    string AccessToken) : IRequest<Result<WhatsAppConfigDto>>;
