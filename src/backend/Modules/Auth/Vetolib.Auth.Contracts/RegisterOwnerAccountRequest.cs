namespace Vetolib.Auth.Contracts;

public record RegisterOwnerAccountRequest(
    string Email,
    string Phone,
    string FullName,
    string Password);
