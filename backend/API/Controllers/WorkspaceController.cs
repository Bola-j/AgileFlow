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
            var workspace = await _workspaceService.GetByIdAsync(id, UserId);
            if (workspace is null)
                return NotFound(new { message = $"Workspace with id {id} not found." });

            return Ok(workspace);
        }

        [HttpPost]
        public async Task<ActionResult<WorkspaceResponse>> Create([FromBody] CreateWorkspaceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _workspaceService.CreateAsync(request, UserId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<WorkspaceResponse>> Update(int id, [FromBody] UpdateWorkspaceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _workspaceService.UpdateAsync(id, request, UserId);
            if (updated is null)
                return NotFound(new { message = $"Workspace with id {id} not found." });

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _workspaceService.DeleteAsync(id, UserId);
            if (!deleted)
                return NotFound(new { message = $"Workspace with id {id} not found." });

            return NoContent();
        }

        [HttpPost("{workspaceId:int}/members")]
        public async Task<IActionResult> AddMember(int workspaceId, [FromBody] AddWorkspaceMemberRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _workspaceService.AddMemberAsync(workspaceId, request, UserId);
            return Ok(new { message = "Member added or restored successfully." });
        }

        [HttpPut("{workspaceId:int}/members/{memberUserId}/role")]
        public async Task<IActionResult> UpdateMemberRole(int workspaceId, string memberUserId, [FromBody] UpdateWorkspaceMemberRoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _workspaceService.UpdateMemberRoleAsync(workspaceId, memberUserId, request, UserId);
            return Ok(new { message = "Member role updated successfully." });
        }

        [HttpDelete("{workspaceId:int}/members/{memberUserId}")]
        public async Task<IActionResult> RemoveMember(int workspaceId, string memberUserId)
        {
            await _workspaceService.RemoveMemberAsync(workspaceId, memberUserId, UserId);
            return Ok(new { message = "Member removed successfully." });
        }

        

        [HttpGet("{workspaceId:int}/members/{memberUserId}")]
        public async Task<ActionResult<WorkspaceMemberDetailResponse>> GetWorkspaceMemberDetail(int workspaceId, string memberUserId)
        {
            var memberDetail = await _workspaceService.GetWorkspaceMemberDetailAsync(workspaceId, memberUserId, UserId);
            return Ok(memberDetail);
        }
    }
}
