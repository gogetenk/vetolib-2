using System.Text.Json;

namespace Vetolib.Auth.Contracts;

public record ReceiveWebhookRequest(string EventType, JsonElement Payload);
