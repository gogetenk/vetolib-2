namespace Vetolib.Auth.Contracts;

public record ReferralCodeDto(
    Guid Id,
    string Code,
    int UsageCount,
    DateTime CreatedAt);
