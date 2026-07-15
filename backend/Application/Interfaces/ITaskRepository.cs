using AgileFlow.Domain.Entities;
using Domain.Entities;

namespace Application.Interfaces;

public interface ITaskRepository
{
    Task<IEnumerable<ProjectTask>> GetBySprintIdAsync(int sprintId);
    Task<ProjectTask?> GetByIdAsync(int id);
    Task<Sprint?> GetSprintByIdAsync(int sprintId);
    Task<BoardColumn?> GetColumnByIdAsync(int columnId);
    Task<ProjectTask> AddAsync(ProjectTask task);
    Task UpdateAsync(ProjectTask task);
    Task DeleteAsync(ProjectTask task);
    Task<UserTask?> GetAssignmentAsync(int taskId, string userId, bool includeDeleted = false);
    Task AddAssignmentAsync(UserTask assignment);
    Task UpdateAssignmentAsync(UserTask assignment);
    Task<TaskDependent?> GetDependencyAsync(int taskId, int dependedTaskId);
    Task AddDependencyAsync(TaskDependent dependency);
    Task RemoveDependencyAsync(TaskDependent dependency);
    Task<List<int>> GetDependedTaskIdsAsync(int taskId);
    Task<Commit?> GetLatestCommitAsync(int taskId);
    Task AddCommitAsync(Commit commit);
    Task UpdateCommitAsync(Commit commit);
    Task AddCommentAsync(Comment comment);
    Task AddActivityLogAsync(TaskActivityLog log);
    Task<IEnumerable<TaskActivityLog>> GetActivityLogsByTaskIdAsync(int taskId);
}
