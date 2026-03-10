namespace Vetolib.Messaging.Contracts;

public record ResponseTemplateDto(
    Guid Id,
    Guid ClinicId,
    string Name,
    string ContentEn,
    string ContentAr,
    string? Category,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
