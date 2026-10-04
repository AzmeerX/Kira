using Kira.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Kira.Api.DTOs;

public class CreateIssueDto
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    public DateTime? DueDate { get; set; }

    [Required]
    public int ProjectId { get; set; }

    public string? AssignedToId { get; set; }
}