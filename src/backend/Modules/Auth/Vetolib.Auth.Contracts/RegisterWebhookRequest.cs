namespace Vetolib.Auth.Contracts;

public record RegisterWebhookRequest(string Name, string Secret, List<string> EventTypes);
