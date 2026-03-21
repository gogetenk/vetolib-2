namespace Vetolib.Messaging.Contracts;

public record WhatsAppTestRequest(
    string RecipientPhone,
    string TemplateName);
