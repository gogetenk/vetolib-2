namespace Vetolib.MedicalRecords.Contracts;

public record CreateOwnerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone);
