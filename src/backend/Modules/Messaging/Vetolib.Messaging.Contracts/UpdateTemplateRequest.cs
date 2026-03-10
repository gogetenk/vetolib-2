namespace Vetolib.Messaging.Contracts;

public record UpdateTemplateRequest(
    string Name,
    string ContentEn,
    string ContentAr,
    MessageCategory? Category
);
