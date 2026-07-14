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
            var projects = await _projectService.GetByWorkspaceIdAsync(workspaceId, UserId);
            return Ok(projects);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProjectResponse>> GetById(int id)
        {
            var project = await _projectService.GetByIdAsync(id, UserId);
            if (project is null)
                return NotFound(new { message = $"Project with id {id} not found." });

            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectResponse>> Create([FromBody] CreateProjectRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _projectService.CreateAsync(request, UserId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProjectResponse>> Update(int id, [FromBody] UpdateProjectRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _projectService.UpdateAsync(id, request, UserId);
            if (updated is null)
                return NotFound(new { message = $"Project with id {id} not found." });

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _projectService.DeleteAsync(id, UserId);
            if (!deleted)
                return NotFound(new { message = $"Project with id {id} not found." });

            return NoContent();
        }
    }
}
