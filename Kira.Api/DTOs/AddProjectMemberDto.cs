using System.ComponentModel.DataAnnotations;

namespace Kira.Api.DTOs;

public class AddProjectMemberDto
{
    public string? UserId { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string Role { get; set; } = "Developer";
}
