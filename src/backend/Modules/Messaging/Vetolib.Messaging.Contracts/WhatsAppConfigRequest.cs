namespace Vetolib.Messaging.Contracts;

public record WhatsAppConfigRequest(
    string WabaId,
    string PhoneNumberId,
    string AccessToken);
