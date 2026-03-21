using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.GetWhatsAppConfig;

internal record GetWhatsAppConfigQuery() : IRequest<Result<WhatsAppConfigDto>>;
