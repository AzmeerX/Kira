using System.ComponentModel.DataAnnotations;
using Kira.Api.Models;

namespace Kira.Api.DTOs;

public class UpdateIssueDto
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public IssueStatus Status { get; set; }

    public IssuePriority Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public string? AssignedToId { get; set; }
}