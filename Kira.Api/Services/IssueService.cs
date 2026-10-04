using Kira.Api.Data;
using Kira.Api.DTOs;
using Kira.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kira.Api.Services;

public class IssueService
{
    private readonly KiraDbContext _db;

    public IssueService(KiraDbContext db)
    {
        _db = db;
    }

    public async Task<List<Issue>> GetByProjectAsync(int projectId)
    {
        return await _db.Issues
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Issue?> GetByIdAsync(int id)
    {
        return await _db.Issues
            .Include(i => i.CreatedBy)
            .Include(i => i.AssignedTo)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Issue?> CreateAsync(
        CreateIssueDto dto,
        string userId)
    {
        var projectExists = await _db.Projects
            .AnyAsync(p => p.Id == dto.ProjectId);

        if (!projectExists)
        {
            return null;
        }

        var issue = new Issue
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            ProjectId = dto.ProjectId,
            CreatedById = userId,
            AssignedToId = dto.AssignedToId
        };

        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        return issue;
    }

    public async Task<Issue?> UpdateAsync(
    int id,
    UpdateIssueDto dto,
    string userId)
    {
        var issue = await _db.Issues.FindAsync(id);

        if (issue is null)
        {
            return null;
        }

        if (issue.Status != dto.Status)
        {
            _db.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id,
                UserId = userId,
                Action = "Status changed",
                OldValue = issue.Status.ToString(),
                NewValue = dto.Status.ToString()
            });
        }

        if (issue.Priority != dto.Priority)
        {
            _db.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id,
                UserId = userId,
                Action = "Priority changed",
                OldValue = issue.Priority.ToString(),
                NewValue = dto.Priority.ToString()
            });
        }

        if (issue.AssignedToId != dto.AssignedToId)
        {
            _db.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id,
                UserId = userId,
                Action = "Assignee changed",
                OldValue = issue.AssignedToId,
                NewValue = dto.AssignedToId
            });
        }

        issue.Title = dto.Title;
        issue.Description = dto.Description;
        issue.Status = dto.Status;
        issue.Priority = dto.Priority;
        issue.DueDate = dto.DueDate;
        issue.AssignedToId = dto.AssignedToId;
        issue.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return issue;
    }

    public async Task<Comment?> AddCommentAsync(
    int issueId,
    string userId,
    CreateCommentDto dto)
    {
        var issueExists = await _db.Issues
            .AnyAsync(i => i.Id == issueId);

        if (!issueExists)
        {
            return null;
        }

        var comment = new Comment
        {
            IssueId = issueId,
            UserId = userId,
            Content = dto.Content
        };

        _db.Comments.Add(comment);

        await _db.SaveChangesAsync();

        return comment;
    }

    public async Task<List<Comment>?> GetCommentsAsync(int issueId)
    {
        var issueExists = await _db.Issues
            .AnyAsync(i => i.Id == issueId);

        if (!issueExists)
        {
            return null;
        }

        return await _db.Comments
            .Include(c => c.User)
            .Where(c => c.IssueId == issueId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<IssueHistory>?> GetHistoryAsync(int issueId)
    {
        var issueExists = await _db.Issues
            .AnyAsync(i => i.Id == issueId);

        if (!issueExists)
        {
            return null;
        }

        return await _db.IssueHistories
            .Include(h => h.User)
            .Where(h => h.IssueId == issueId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }
}