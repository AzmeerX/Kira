namespace Kira.Api.DTOs;

public sealed record ProjectDto(
    int Id,
    string Name,
    string Description,
    DateTime CreatedAt);
