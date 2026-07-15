using Application.DTOs.Dashboard;

namespace Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(string userId);
    Task<List<MyTaskResponse>> GetMyTasksAsync(string userId);
}
