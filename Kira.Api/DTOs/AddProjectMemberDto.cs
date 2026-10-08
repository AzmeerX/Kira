using System.ComponentModel.DataAnnotations;

namespace Kira.Api.DTOs;

public class AddProjectMemberDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Developer";
}