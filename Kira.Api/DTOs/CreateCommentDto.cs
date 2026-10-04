using System.ComponentModel.DataAnnotations;

namespace Kira.Api.DTOs;

public class CreateCommentDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}