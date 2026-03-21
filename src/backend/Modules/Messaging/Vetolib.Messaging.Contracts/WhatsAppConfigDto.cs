namespace Vetolib.Messaging.Contracts;

public record WhatsAppConfigDto(
    Guid Id,
    string WabaId,
    string PhoneNumberId,
    bool HasAccessToken,
    DateTime CreatedAt,
    DateTime UpdatedAt);
