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
}