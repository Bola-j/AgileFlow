using Application.DTOs.Workspace;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class WorkspacesController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspacesController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }


        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkspaceSummaryResponse>>> GetMyWorkspaces()
        {
            var workspaces = await _workspaceService.GetMyWorkspacesAsync(UserId);
            return Ok(workspaces);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<WorkspaceResponse>> GetById(int id)
        {
            try
            {
                var workspace = await _workspaceService.GetByIdAsync(id, UserId);
                if (workspace is null)
                    return NotFound(new { message = $"Workspace with id {id} not found." });

                return Ok(workspace);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<WorkspaceResponse>> Create([FromBody] CreateWorkspaceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _workspaceService.CreateAsync(request, UserId);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<WorkspaceResponse>> Update(int id, [FromBody] UpdateWorkspaceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _workspaceService.UpdateAsync(id, request, UserId);
                if (updated is null)
                    return NotFound(new { message = $"Workspace with id {id} not found." });

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
                var deleted = await _workspaceService.DeleteAsync(id, UserId);
                if (!deleted)
                    return NotFound(new { message = $"Workspace with id {id} not found." });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
    }
}
