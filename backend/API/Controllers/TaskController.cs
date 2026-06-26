using Application.DTOs.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("sprints/{sprintId:int}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskSummaryResponse>>> GetBySprint(int sprintId)
    {
        try
        {
            var tasks = await _taskService.GetBySprintAsync(sprintId, UserId);
            return Ok(tasks);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpGet("tasks/{id:int}")]
    public async Task<ActionResult<TaskDetailResponse>> GetById(int id)
    {
        try
        {
            var task = await _taskService.GetByIdAsync(id, UserId);
            if (task is null)
                return NotFound(new { message = $"Task with id {id} not found." });

            return Ok(task);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPost("sprints/{sprintId:int}/tasks")]
    public async Task<ActionResult<TaskDetailResponse>> Create(int sprintId, [FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await _taskService.CreateAsync(sprintId, request, UserId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("tasks/{id:int}")]
    public async Task<ActionResult<TaskDetailResponse>> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _taskService.UpdateAsync(id, request, UserId);
            if (updated is null)
                return NotFound(new { message = $"Task with id {id} not found." });

            return Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("tasks/{id:int}/status")]
    public async Task<ActionResult<TaskDetailResponse>> UpdateStatus(int id, [FromBody] UpdateTaskStatusRequest request)
    {
        try
        {
            var updated = await _taskService.UpdateStatusAsync(id, request, UserId);
            if (updated is null)
                return NotFound(new { message = $"Task with id {id} not found." });

            return Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPut("tasks/{id:int}/move")]
    public async Task<ActionResult<TaskDetailResponse>> Move(int id, [FromBody] MoveTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _taskService.MoveAsync(id, request, UserId);
            if (updated is null)
                return NotFound(new { message = $"Task with id {id} not found." });

            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("tasks/{id:int}/assignees")]
    public async Task<ActionResult<TaskDetailResponse>> AssignUser(int id, [FromBody] AssignTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _taskService.AssignUserAsync(id, request, UserId);
            if (updated is null)
                return NotFound(new { message = $"Task with id {id} not found." });

            return Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpDelete("tasks/{id:int}/assignees/{assigneeUserId}")]
    public async Task<ActionResult<TaskDetailResponse>> UnassignUser(int id, string assigneeUserId)
    {
        try
        {
            var updated = await _taskService.UnassignUserAsync(id, assigneeUserId, UserId);
            if (updated is null)
                return NotFound(new { message = $"Task with id {id} not found." });

            return Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpDelete("tasks/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _taskService.DeleteAsync(id, UserId);
            if (!deleted)
                return NotFound(new { message = $"Task with id {id} not found." });

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }
}
