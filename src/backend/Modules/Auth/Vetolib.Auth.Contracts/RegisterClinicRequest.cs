namespace Vetolib.Auth.Contracts;

public record RegisterClinicRequest(
    string ClinicName,
    string Email,
    string Password,
    string Phone,
    string Country);
