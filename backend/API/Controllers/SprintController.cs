using Application.DTOs.Sprint;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class SprintController : ControllerBase
{
    private readonly ISprintService _sprintService;

    public SprintController(ISprintService sprintService)
    {
        _sprintService = sprintService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("projects/{projectId:int}/sprints")]
    public async Task<ActionResult<IEnumerable<SprintResponse>>> GetByProject(int projectId)
    {
        var sprints = await _sprintService.GetByProjectIdAsync(projectId, UserId);
        return Ok(sprints);
    }

    [HttpGet("sprints/{id:int}")]
    public async Task<ActionResult<SprintResponse>> GetById(int id)
    {
        var sprint = await _sprintService.GetByIdAsync(id, UserId);
        if (sprint is null)
            return NotFound(new { message = $"Sprint with id {id} not found." });

        return Ok(sprint);
    }

    [HttpPost("projects/{projectId:int}/sprints")]
    public async Task<ActionResult<SprintResponse>> Create(int projectId, [FromBody] CreateSprintRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _sprintService.CreateAsync(projectId, request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("sprints/{id:int}")]
    public async Task<ActionResult<SprintResponse>> Update(int id, [FromBody] UpdateSprintRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _sprintService.UpdateAsync(id, request, UserId);
        if (updated is null)
            return NotFound(new { message = $"Sprint with id {id} not found." });

        return Ok(updated);
    }

    [HttpPut("sprints/{id:int}/start")]
    public async Task<ActionResult<SprintResponse>> Start(int id)
    {
        var updated = await _sprintService.StartAsync(id, UserId);
        if (updated is null)
            return NotFound(new { message = $"Sprint with id {id} not found." });

        return Ok(updated);
    }

    [HttpPut("sprints/{id:int}/complete")]
    public async Task<ActionResult<SprintResponse>> Complete(int id)
    {
        var updated = await _sprintService.CompleteAsync(id, UserId);
        if (updated is null)
            return NotFound(new { message = $"Sprint with id {id} not found." });

        return Ok(updated);
    }

    [HttpGet("sprints/{id:int}/progress")]
    public async Task<ActionResult<SprintProgressResponse>> GetProgress(int id)
    {
        var progress = await _sprintService.GetProgressAsync(id, UserId);
        if (progress is null)
            return NotFound(new { message = $"Sprint with id {id} not found." });

        return Ok(progress);
    }
}
