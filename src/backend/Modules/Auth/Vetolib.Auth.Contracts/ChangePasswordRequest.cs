namespace Vetolib.Auth.Contracts;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
