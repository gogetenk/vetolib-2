namespace Vetolib.Messaging.Contracts;

public record CreateTemplateRequest(
    string Name,
    string ContentEn,
    string ContentAr,
    MessageCategory? Category
);
