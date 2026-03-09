namespace Vetolib.Auth.Contracts;

public record AuthTokenDto(
    string AccessToken,
    string RefreshToken,
    UserDto User);
