using Application.DTOs.Project;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("workspace/{workspaceId:int}")]
        public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetByWorkspace(int workspaceId)
        {
            try
            {
                var projects = await _projectService.GetByWorkspaceIdAsync(workspaceId, UserId);
                return Ok(projects);
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProjectResponse>> GetById(int id)
        {
            try
            {
                var project = await _projectService.GetByIdAsync(id, UserId);
                if (project is null)
                    return NotFound(new { message = $"Project with id {id} not found." });

                return Ok(project);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProjectResponse>> Create([FromBody] CreateProjectRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _projectService.CreateAsync(request, UserId);
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

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProjectResponse>> Update(int id, [FromBody] UpdateProjectRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _projectService.UpdateAsync(id, request, UserId);
                if (updated is null)
                    return NotFound(new { message = $"Project with id {id} not found." });

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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _projectService.DeleteAsync(id, UserId);
                if (!deleted)
                    return NotFound(new { message = $"Project with id {id} not found." });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
    }
}
