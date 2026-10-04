using System.Security.Claims;
using Kira.Api.DTOs;
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

    public IssuesController(IssueService issueService)
    {
        _issueService = issueService;
    }

    [HttpGet("project/{projectId:int}")]
    public async Task<ActionResult> GetByProject(int projectId)
    {
        var issues = await _issueService.GetByProjectAsync(projectId);

        return Ok(issues);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var issue = await _issueService.GetByIdAsync(id);

        if (issue is null)
        {
            return NotFound();
        }

        return Ok(issue);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateIssueDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Unauthorized();
        }

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
}