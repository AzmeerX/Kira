namespace Kira.Api.Models;

public enum IssueStatus
{
    Todo,
    InProgress,
    InReview,
    Done
}

public enum IssuePriority
{
    Low,
    Medium,
    High,
    Urgent
}

public class Issue
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IssueStatus Status { get; set; } = IssueStatus.Todo;

    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Project
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Creator
    public string CreatedById { get; set; } = string.Empty;
    public ApplicationUser CreatedBy { get; set; } = null!;

    // Assignee
    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }

    public ICollection<Comment> Comments { get; set; }
    = new List<Comment>();

    public ICollection<IssueHistory> History { get; set; }
        = new List<IssueHistory>();
}
