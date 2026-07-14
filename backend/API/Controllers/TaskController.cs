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
        var tasks = await _taskService.GetBySprintAsync(sprintId, UserId);
        return Ok(tasks);
    }

    [HttpGet("tasks/{id:int}")]
    public async Task<ActionResult<TaskDetailResponse>> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id, UserId);
        if (task is null)
            return NotFound(new { message = $"Task with id {id} not found." });

        return Ok(task);
    }

    [HttpPost("sprints/{sprintId:int}/tasks")]
    public async Task<ActionResult<TaskDetailResponse>> Create(int sprintId, [FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _taskService.CreateAsync(sprintId, request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("tasks/{id:int}")]
    public async Task<ActionResult<TaskDetailResponse>> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _taskService.UpdateAsync(id, request, UserId);
        if (updated is null)
            return NotFound(new { message = $"Task with id {id} not found." });

        return Ok(updated);
    }

    [HttpPatch("tasks/{id:int}/status")]
    public async Task<ActionResult<TaskDetailResponse>> UpdateStatus(int id, [FromBody] UpdateTaskStatusRequest request)
    {
        var updated = await _taskService.UpdateStatusAsync(id, request, UserId);
        if (updated is null)
            return NotFound(new { message = $"Task with id {id} not found." });

        return Ok(updated);
    }

    [HttpPut("tasks/{id:int}/move")]
    public async Task<ActionResult<TaskDetailResponse>> Move(int id, [FromBody] MoveTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _taskService.MoveAsync(id, request, UserId);
        if (updated is null)
            return NotFound(new { message = $"Task with id {id} not found." });

        return Ok(updated);
    }

    [HttpPost("tasks/{id:int}/assignees")]
    public async Task<ActionResult<TaskDetailResponse>> AssignUser(int id, [FromBody] AssignTaskRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _taskService.AssignUserAsync(id, request, UserId);
        if (updated is null)
            return NotFound(new { message = $"Task with id {id} not found." });

        return Ok(updated);
    }

    [HttpDelete("tasks/{id:int}/assignees/{assigneeUserId}")]
    public async Task<ActionResult<TaskDetailResponse>> UnassignUser(int id, string assigneeUserId)
    {
        var updated = await _taskService.UnassignUserAsync(id, assigneeUserId, UserId);
        if (updated is null)
            return NotFound(new { message = $"Task with id {id} not found." });

        return Ok(updated);
    }

    [HttpDelete("tasks/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _taskService.DeleteAsync(id, UserId);
        if (!deleted)
            return NotFound(new { message = $"Task with id {id} not found." });

        return NoContent();
    }

    [HttpPost("tasks/{id:int}/dependencies/{dependencyTaskId:int}")]
    public async Task<IActionResult> AddDependency(int id, int dependencyTaskId)
    {
        await _taskService.AddDependencyAsync(id, dependencyTaskId, UserId);
        return Ok(new { message = "Dependency added successfully." });
    }

    [HttpDelete("tasks/{id:int}/dependencies/{dependencyTaskId:int}")]
    public async Task<IActionResult> RemoveDependency(int id, int dependencyTaskId)
    {
        await _taskService.RemoveDependencyAsync(id, dependencyTaskId, UserId);
        return Ok(new { message = "Dependency removed successfully." });
    }

    [HttpGet("tasks/{id:int}/activity-logs")]
    public async Task<ActionResult<IEnumerable<TaskActivityLogResponse>>> GetActivityLogs(int id)
    {
        var logs = await _taskService.GetActivityLogsAsync(id, UserId);
        return Ok(logs);
    }
}
