namespace Vetolib.Auth.Contracts;

public record UserPagedResultDto(
    IReadOnlyList<UserListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
