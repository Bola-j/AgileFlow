using Application.DTOs.Board;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _boardService;

        public BoardController(IBoardService boardService)
        {
            _boardService = boardService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("projects/{projectId:int}/board")]
        public async Task<ActionResult<GetBoardDetailsResponse>> GetBoardDetails(int projectId, [FromQuery] int sprintId)
        {
            var boardDetails = await _boardService.GetBoardDetailsAsync(projectId, sprintId, UserId);
            if (boardDetails is null)
                return NotFound(new { message = $"Board for project {projectId} was not found." });

            return Ok(boardDetails);
        }

        [HttpPost("projects/{projectId:int}/board/columns")]
        public async Task<IActionResult> AddColumn(int projectId, [FromBody] AddColumnRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _boardService.AddColumnAsync(projectId, request, UserId);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut("columns/{columnId:int}")]
        public async Task<IActionResult> UpdateColumnName(int columnId, [FromBody] UpdateColumnRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _boardService.UpdateColumnNameAsync(columnId, request, UserId);
            return Ok(new { message = "Column name updated successfully." });
        }

        [HttpDelete("columns/{columnId:int}")]
        public async Task<IActionResult> DeleteColumn(int columnId)
        {
            await _boardService.DeleteColumnAsync(columnId, UserId);
            return NoContent();
        }

        [HttpPut("projects/{projectId:int}/board/columns/order")]
        public async Task<IActionResult> UpdateColumnsOrder(int projectId, [FromBody] UpdateColumnOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _boardService.UpdateColumnsOrderAsync(projectId, request, UserId);
            return Ok(new { message = "Columns order updated successfully." });
        }
    }
}
