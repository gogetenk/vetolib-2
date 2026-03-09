namespace Vetolib.Auth.Contracts;

public record RegisterClinicResponse(
    string AccessToken,
    string RefreshToken,
    Guid ClinicId,
    string ClinicName,
    UserDto User);
