using Application.DTOs.Dashboard;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> Summary()
    {
        return Ok(await _dashboardService.GetSummaryAsync(UserId));
    }

    [HttpGet("my-tasks")]
    public async Task<ActionResult<IEnumerable<MyTaskResponse>>> MyTasks()
    {
        return Ok(await _dashboardService.GetMyTasksAsync(UserId));
    }
}
