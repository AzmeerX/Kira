using Kira.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Kira.Api.Services;

public class ProjectPermissionService
{
    private readonly KiraDbContext _db;

    public ProjectPermissionService(KiraDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsMemberAsync(
        int projectId,
        string userId)
    {
        return await _db.ProjectMembers.AnyAsync(pm =>
            pm.ProjectId == projectId &&
            pm.UserId == userId);
    }

    public async Task<bool> IsManagerAsync(
        int projectId,
        string userId)
    {
        return await _db.ProjectMembers.AnyAsync(pm =>
            pm.ProjectId == projectId &&
            pm.UserId == userId &&
            pm.Role == "Manager");
    }

    public async Task<bool> IsMemberOfIssueAsync(int issueId, string userId)
    {
        return await _db.Issues.AnyAsync(i =>
            i.Id == issueId &&
            i.Project.Members.Any(pm => pm.UserId == userId));
    }
}
