using Kira.Api.Data;
using Kira.Api.DTOs;
using Kira.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kira.Api.Services;

public class ProjectService
{
    private readonly KiraDbContext _db;

    public ProjectService(KiraDbContext db)
    {
        _db = db;
    }

    public async Task<List<Project>> GetAllAsync(string userId)
    {
        return await _db.Projects
            .AsNoTracking()
            .Where(p => p.Members.Any(pm => pm.UserId == userId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> CreateAsync(CreateProjectDto dto, string creatorId)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _db.Projects.Add(project);
        project.Members.Add(new ProjectMember
        {
            UserId = creatorId,
            Role = "Manager"
        });
        await _db.SaveChangesAsync();

        return project;
    }

    public async Task<Project?> UpdateAsync(int id, UpdateProjectDto dto)
    {
        var project = await _db.Projects.FindAsync(id);

        if (project is null)
        {
            return null;
        }

        project.Name = dto.Name;
        project.Description = dto.Description;

        await _db.SaveChangesAsync();

        return project;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _db.Projects.FindAsync(id);

        if (project is null)
        {
            return false;
        }

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddMemberAsync(
    int projectId,
    string userId,
    string role)
    {
        var projectExists = await _db.Projects
            .AnyAsync(p => p.Id == projectId);

        var userExists = await _db.Users
            .AnyAsync(u => u.Id == userId);

        if (!projectExists || !userExists)
        {
            return false;
        }

        if (role != "Manager" && role != "Developer")
        {
            return false;
        }

        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm =>
                pm.ProjectId == projectId &&
                pm.UserId == userId);

        if (member is not null)
        {
            if (member.Role == "Manager" && role != "Manager")
            {
                var managerCount = await _db.ProjectMembers
                    .CountAsync(pm => pm.ProjectId == projectId && pm.Role == "Manager");

                if (managerCount <= 1)
                {
                    return false;
                }
            }

            member.Role = role;
        }
        else
        {
            _db.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                Role = role
            });
        }

        await _db.SaveChangesAsync();

        return true;
    }
}
