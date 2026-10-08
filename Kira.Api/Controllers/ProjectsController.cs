using System.Security.Claims;
using Kira.Api.DTOs;
using Kira.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kira.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;
    private readonly ProjectPermissionService _permissionService;

    public ProjectsController(
        ProjectService projectService,
        ProjectPermissionService permissionService)
    {
        _projectService = projectService;
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult> GetProjects()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var projects = await _projectService.GetAllAsync(userId);

        return Ok(projects);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetProject(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        if (!await _permissionService.IsMemberAsync(id, userId)) return Forbid();

        var project = await _projectService.GetByIdAsync(id);

        if (project is null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult> CreateProject(CreateProjectDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var project = await _projectService.CreateAsync(dto, userId);

        return CreatedAtAction(
            nameof(GetProject),
            new { id = project.Id },
            project
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProject(
        int id,
        UpdateProjectDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        if (!await _permissionService.IsManagerAsync(id, userId)) return Forbid();

        var project = await _projectService.UpdateAsync(id, dto);

        if (project is null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        if (!await _permissionService.IsManagerAsync(id, userId)) return Forbid();

        var deleted = await _projectService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:int}/members")]
    public async Task<ActionResult> AddMember(
    int id,
    AddProjectMemberDto dto)
    {
        var currentUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var isManager = await _permissionService.IsManagerAsync(
            id,
            currentUserId);

        if (!isManager)
        {
            return Forbid();
        }

        var success = await _projectService.AddMemberAsync(
            id,
            dto.UserId,
            dto.Role);

        if (!success)
        {
            return BadRequest(new
            {
                message = "The project or user was not found, the role is invalid, or the project must retain at least one manager."
            });
        }

        return NoContent();
    }
}
