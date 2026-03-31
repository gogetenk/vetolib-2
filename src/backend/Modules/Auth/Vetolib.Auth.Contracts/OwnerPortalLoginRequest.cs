namespace Vetolib.Auth.Contracts;

public record OwnerPortalLoginRequest(
    string Email,
    string Password);
