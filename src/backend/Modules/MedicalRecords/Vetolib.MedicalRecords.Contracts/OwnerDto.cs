namespace Vetolib.MedicalRecords.Contracts;

public record OwnerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Guid ClinicId);
