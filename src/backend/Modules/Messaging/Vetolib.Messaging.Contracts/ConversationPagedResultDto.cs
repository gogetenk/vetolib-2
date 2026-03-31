namespace Vetolib.Messaging.Contracts;

public record ConversationPagedResultDto(
    IReadOnlyList<ConversationDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
