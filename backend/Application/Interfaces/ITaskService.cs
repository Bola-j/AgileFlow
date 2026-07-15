using Application.DTOs.Tasks;

namespace Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskSummaryResponse>> GetBySprintAsync(int sprintId, string userId);
    Task<TaskDetailResponse?> GetByIdAsync(int id, string userId);
    Task<TaskDetailResponse> CreateAsync(int sprintId, CreateTaskRequest request, string userId);
    Task<TaskDetailResponse?> UpdateAsync(int id, UpdateTaskRequest request, string userId);
    Task<TaskDetailResponse?> UpdateStatusAsync(int id, UpdateTaskStatusRequest request, string userId);
    Task<TaskDetailResponse?> SubmitAsync(int id, SubmitTaskRequest request, string userId);
    Task<TaskDetailResponse?> ReviewAsync(int id, ReviewTaskRequest request, string userId);
    Task<TaskDetailResponse?> MoveAsync(int id, MoveTaskRequest request, string userId);
    Task<TaskDetailResponse?> AssignUserAsync(int id, AssignTaskRequest request, string userId);
    Task<TaskDetailResponse?> UnassignUserAsync(int id, string assigneeUserId, string userId);
    Task<bool> DeleteAsync(int id, string userId);
    Task<bool> AddDependencyAsync(int taskId, int dependencyTaskId, string userId);
    Task<bool> RemoveDependencyAsync(int taskId, int dependencyTaskId, string userId);
    Task<IEnumerable<TaskActivityLogResponse>> GetActivityLogsAsync(int taskId, string userId);
}
