using System.Security.Claims;
using Kira.Api.DTOs;
using Kira.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Kira.Api.Models;

namespace Kira.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;
    private readonly ProjectPermissionService _permissionService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsController(
        ProjectService projectService,
        ProjectPermissionService permissionService,
        UserManager<ApplicationUser> userManager)
    {
        _projectService = projectService;
        _permissionService = permissionService;
        _userManager = userManager;
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
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var project = await _projectService.CreateAsync(dto, userId);

        return CreatedAtAction(
            nameof(GetProject),
            new { id = project.Id },
            new ProjectDto(
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAt)
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

        var targetUserId = dto.UserId;
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var targetUser = await _userManager.FindByEmailAsync(dto.Email);
            targetUserId = targetUser?.Id;
        }

        var success = !string.IsNullOrWhiteSpace(targetUserId) &&
            await _projectService.AddMemberAsync(id, targetUserId, dto.Role);

        if (!success)
        {
            return BadRequest(new
            {
                message = "The project or user was not found, the role is invalid, or the project must retain at least one manager."
            });
        }

        return NoContent();
    }

    [HttpGet("{id:int}/members")]
    public async Task<ActionResult> GetMembers(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null) return Unauthorized();
        if (!await _permissionService.IsMemberAsync(id, currentUserId)) return NotFound();

        return Ok(await _projectService.GetMembersAsync(id));
    }

    [HttpDelete("{id:int}/members/{memberId}")]
    public async Task<ActionResult> RemoveMember(
        int id,
        string memberId,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null) return Unauthorized();
        if (!await _permissionService.IsManagerAsync(id, currentUserId)) return Forbid();
        if (!await _permissionService.IsMemberAsync(id, memberId)) return NotFound();

        var removed = await _projectService.RemoveMemberAsync(id, memberId, cancellationToken);
        if (!removed)
        {
            return Conflict(new
            {
                message = "The project must retain at least one manager."
            });
        }

        return NoContent();
    }
}
