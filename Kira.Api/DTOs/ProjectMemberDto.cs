namespace Kira.Api.DTOs;

public sealed record ProjectMemberDto(
    string UserId,
    string DisplayName,
    string? Email,
    string Role);
