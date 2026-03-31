namespace Vetolib.Auth.Contracts;

public record OwnerPortalTokenDto(
    string AccessToken,
    string RefreshToken,
    OwnerAccountDto Account,
    Guid[] LinkedClinicIds);
