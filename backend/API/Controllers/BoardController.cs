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

        [HttpPost("projects/{projectId:int}/boards")]
        public async Task<ActionResult<CreateBoardResponse>> Create(int projectId,[FromBody] CreateBoardRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdBoard = await _boardService.CreateBoardAsync(projectId, request, UserId);
                return Ok(createdBoard);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpGet("projects/{projectId:int}/boards")]
        public async Task<ActionResult<IEnumerable<BoardSummaryResponse>>> GetProjectBoards(int projectId)
        {
            try
            {
                var boards = await _boardService.GetProjectBoardsAsync(projectId, UserId);
                return Ok(boards);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpGet("boards/{boardId:int}")]
        public async Task<ActionResult<GetBoardDetailsResponse>> GetBoardDetails(int boardId)
        {
            try
            {
                var boardDetails = await _boardService.GetBoardDetailsAsync(boardId, UserId);

                if (boardDetails is null)
                    return NotFound(new { message = $"Board with id {boardId} not found." });

                return Ok(boardDetails);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpPost("boards/{boardId:int}/columns")]
        public async Task<IActionResult> AddColumn(int boardId,[FromBody] AddColumnRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _boardService.AddColumnAsync(boardId, request, UserId);
                return Ok(new { message = "Column added successfully." });
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

        [HttpPut("boards/{boardId:int}/columns/order")]
        public async Task<IActionResult> UpdateColumnsOrder(int boardId,[FromBody] UpdateColumnOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _boardService.UpdateColumnsOrderAsync(boardId, request, UserId);
                return Ok(new { message = "Columns order updated successfully." });
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
    }
}
