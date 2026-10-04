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

    public async Task<List<Project>> GetAllAsync()
    {
        return await _db.Projects
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _db.Projects.FindAsync(id);
    }

    public async Task<Project> CreateAsync(CreateProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _db.Projects.Add(project);
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
}