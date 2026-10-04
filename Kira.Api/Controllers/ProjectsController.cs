using Kira.Api.Data;
using Kira.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kira.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly KiraDbContext _db;

    public ProjectsController(KiraDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Project>>> GetProjects()
    {
        var projects = await _db.Projects.ToListAsync();

        return Ok(projects);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return Ok(project);
    }
}