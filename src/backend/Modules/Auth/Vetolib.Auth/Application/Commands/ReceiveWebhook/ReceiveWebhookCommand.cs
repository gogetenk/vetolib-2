using System.Text.Json;
using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.ReceiveWebhook;

internal record ReceiveWebhookCommand(string Signature, string RawBody, string EventType, JsonElement Payload)
    : IRequest<Result>;
