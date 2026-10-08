using System.Security.Claims;
using Kira.Api.DTOs;
using Kira.Api.Models;
using Kira.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kira.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class IssuesController : ControllerBase
{
    private readonly IssueService _issueService;
    private readonly ProjectPermissionService _permissionService;

    public IssuesController(IssueService issueService, ProjectPermissionService permissionService)
    {
        _issueService = issueService;
        _permissionService = permissionService;
    }

    [HttpGet("project/{projectId:int}")]
    public async Task<ActionResult> GetByProject(
    int projectId,
    [FromQuery] string? search,
    [FromQuery] IssueStatus? status,
    [FromQuery] IssuePriority? priority,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        if (!await _permissionService.IsMemberAsync(projectId, userId)) return NotFound();

        if (page < 1)
        {
            return BadRequest("Page must be at least 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Page size must be between 1 and 100.");
        }

        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            return BadRequest("Status is not a valid issue status.");
        }

        if (priority.HasValue && !Enum.IsDefined(priority.Value))
        {
            return BadRequest("Priority is not a valid issue priority.");
        }

        var result = await _issueService.GetByProjectAsync(
            projectId,
            search,
            status,
            priority,
            page,
            pageSize);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        if (!await _permissionService.IsMemberOfIssueAsync(id, userId)) return NotFound();

        var issue = await _issueService.GetByIdAsync(id);

        if (issue is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            issue.Id,
            issue.Title,
            issue.Description,
            issue.Status,
            issue.Priority,
            issue.DueDate,
            issue.CreatedAt,
            issue.UpdatedAt,
            issue.ProjectId,
            issue.CreatedById,
            CreatedBy = new { issue.CreatedBy.Id, issue.CreatedBy.DisplayName, issue.CreatedBy.Email },
            issue.AssignedToId,
            AssignedTo = issue.AssignedTo is null
                ? null
                : new { issue.AssignedTo.Id, issue.AssignedTo.DisplayName, issue.AssignedTo.Email }
        });
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateIssueDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Unauthorized();
        }

        if (!await _permissionService.IsMemberAsync(dto.ProjectId, userId)) return NotFound();
        if (dto.AssignedToId is not null &&
            !await _permissionService.IsMemberAsync(dto.ProjectId, dto.AssignedToId))
            return BadRequest("Assignee must be a member of the project.");

        var issue = await _issueService.CreateAsync(dto, userId);

        if (issue is null)
        {
            return NotFound(new
            {
                message = "Project not found."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = issue.Id },
            issue
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(
    int id,
    UpdateIssueDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Unauthorized();
        }

        if (!await _permissionService.IsMemberOfIssueAsync(id, userId)) return NotFound();
        if (dto.AssignedToId is not null)
        {
            var issueProjectId = await _issueService.GetProjectIdAsync(id);
            if (issueProjectId is null ||
                !await _permissionService.IsMemberAsync(issueProjectId.Value, dto.AssignedToId))
                return BadRequest("Assignee must be a member of the project.");
        }

        var issue = await _issueService.UpdateAsync(
            id,
            dto,
            userId);

        if (issue is null)
        {
            return NotFound();
        }

        return Ok(issue);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult> AddComment(
    int id,
    CreateCommentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Unauthorized();
        }

        if (!await _permissionService.IsMemberOfIssueAsync(id, userId)) return NotFound();

        var comment = await _issueService.AddCommentAsync(
            id,
            userId,
            dto);

        if (comment is null)
        {
            return NotFound();
        }

        return Ok(comment);
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult> GetComments(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        if (!await _permissionService.IsMemberOfIssueAsync(id, userId)) return NotFound();

        var comments = await _issueService.GetCommentsAsync(id);

        if (comments is null)
        {
            return NotFound();
        }

        return Ok(comments.Select(comment => new
        {
            comment.Id,
            comment.Content,
            comment.CreatedAt,
            comment.IssueId,
            comment.UserId,
            User = new { comment.User.Id, comment.User.DisplayName, comment.User.Email }
        }));
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult> GetHistory(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        if (!await _permissionService.IsMemberOfIssueAsync(id, userId)) return NotFound();

        var history = await _issueService.GetHistoryAsync(id);

        if (history is null)
        {
            return NotFound();
        }

        return Ok(history.Select(entry => new
        {
            entry.Id,
            entry.Action,
            entry.OldValue,
            entry.NewValue,
            entry.CreatedAt,
            entry.IssueId,
            entry.UserId,
            User = new { entry.User.Id, entry.User.DisplayName, entry.User.Email }
        }));
    }
}
