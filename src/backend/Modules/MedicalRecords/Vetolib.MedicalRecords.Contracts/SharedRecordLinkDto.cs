namespace Vetolib.MedicalRecords.Contracts;

public record SharedRecordLinkDto(
    Guid Id,
    Guid PatientId,
    string PatientName,
    string Token,
    string ShareUrl,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    int AccessCount,
    DateTime CreatedAt);

public record CreateShareLinkResponse(
    Guid Id,
    string Token,
    string ShareUrl,
    DateTime ExpiresAt);
