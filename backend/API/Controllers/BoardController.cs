using Application.DTOs.Board;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public async Task<ActionResult<GetBoardDetailsResponse>> GetBoardDetails(int projectId)
        {
            try
            {
                var boardDetails = await _boardService.GetBoardDetailsAsync(projectId, UserId);

                if (boardDetails is null)
                    return NotFound(new { message = $"Board for project {projectId} was not found." });

                return Ok(boardDetails);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpPost("projects/{projectId:int}/board/columns")]
        public async Task<IActionResult> AddColumn(int projectId, [FromBody] AddColumnRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _boardService.AddColumnAsync(projectId, request, UserId);
                //return Ok(new { message = "Column added successfully." });
                return StatusCode(StatusCodes.Status201Created);
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

        [HttpPut("columns/{columnId:int}")]
        public async Task<IActionResult> UpdateColumnName(int columnId,[FromBody] UpdateColumnRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _boardService.UpdateColumnNameAsync(columnId, request, UserId);
                return Ok(new { message = "Column name updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpDelete("columns/{columnId:int}")]
        public async Task<IActionResult> DeleteColumn(int columnId)
        {
            try
            {
                await _boardService.DeleteColumnAsync(columnId, UserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpPut("projects/{projectId:int}/board/columns/order")]
        public async Task<IActionResult> UpdateColumnsOrder(int projectId, [FromBody] UpdateColumnOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _boardService.UpdateColumnsOrderAsync(projectId, request, UserId);
                return Ok(new { message = "Columns order updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
    }
}
