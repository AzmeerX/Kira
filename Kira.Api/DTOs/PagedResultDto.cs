namespace Kira.Api.DTOs;

public sealed record PagedResultDto<T>(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<T> Items);
