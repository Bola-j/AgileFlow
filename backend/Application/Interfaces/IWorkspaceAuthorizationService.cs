using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces;

public interface IWorkspaceAuthorizationService
{
    Task<UserWorkspace> EnsureMemberAsync(int workspaceId, string userId);
    Task<UserWorkspace> EnsureRoleAsync(int workspaceId, string userId, params UserRole[] allowedRoles);
    Task<UserWorkspace> EnsureProjectRoleAsync(int projectId, string userId, params UserRole[] allowedRoles);
    Task<UserWorkspace> EnsureSprintRoleAsync(int sprintId, string userId, params UserRole[] allowedRoles);
    Task<UserWorkspace> EnsureTaskRoleAsync(int taskId, string userId, params UserRole[] allowedRoles);
    Task EnsureProjectMemberAsync(int projectId, string userId);
    Task EnsureSprintMemberAsync(int sprintId, string userId);
    Task EnsureTaskMemberAsync(int taskId, string userId);
    Task<int> GetWorkspaceIdForProjectAsync(int projectId);
    Task<int> GetWorkspaceIdForSprintAsync(int sprintId);
    Task<int> GetWorkspaceIdForTaskAsync(int taskId);
    Task<bool> IsTaskAssigneeAsync(int taskId, string userId);
}
