using Ardalis.Result;
using MediatR;
using Vetolib.Auth.Contracts;

namespace Vetolib.Auth.Application.Queries.ListWebhooks;

internal record ListWebhooksQuery() : IRequest<Result<List<WebhookRegistrationDto>>>;
