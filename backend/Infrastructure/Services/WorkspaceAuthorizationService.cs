using Application.Interfaces;
using AgileFlow.Infrastructure.Persistence.Data;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class WorkspaceAuthorizationService : IWorkspaceAuthorizationService
{
    private readonly AgileFlowDbContext _context;

    public WorkspaceAuthorizationService(AgileFlowDbContext context)
    {
        _context = context;
    }

    public async Task<UserWorkspace> EnsureMemberAsync(int workspaceId, string userId)
    {
        var membership = await _context.UserWorkspaces
            .FirstOrDefaultAsync(uw => uw.WorkspaceId == workspaceId &&
                                       uw.AppUserId == userId &&
                                       !uw.IsDeleted);

        if (membership is null)
            throw new UnauthorizedAccessException("You are not a member of this workspace.");

        return membership;
    }

    public async Task<UserWorkspace> EnsureRoleAsync(int workspaceId, string userId, params UserRole[] allowedRoles)
    {
        var membership = await EnsureMemberAsync(workspaceId, userId);

        if (!allowedRoles.Contains(membership.Role))
        {
            var roleNames = string.Join(" or ", allowedRoles.Select(role => role.ToString()));
            throw new UnauthorizedAccessException($"Only {roleNames} can perform this action.");
        }

        return membership;
    }

    public async Task EnsureProjectMemberAsync(int projectId, string userId)
    {
        var workspaceId = await GetWorkspaceIdForProjectAsync(projectId);
        await EnsureMemberAsync(workspaceId, userId);
    }

    public async Task EnsureSprintMemberAsync(int sprintId, string userId)
    {
        var workspaceId = await GetWorkspaceIdForSprintAsync(sprintId);
        await EnsureMemberAsync(workspaceId, userId);
    }

    public async Task EnsureTaskMemberAsync(int taskId, string userId)
    {
        var workspaceId = await GetWorkspaceIdForTaskAsync(taskId);
        await EnsureMemberAsync(workspaceId, userId);
    }

    public async Task<UserWorkspace> EnsureProjectRoleAsync(int projectId, string userId, params UserRole[] allowedRoles)
    {
        var workspaceId = await GetWorkspaceIdForProjectAsync(projectId);
        return await EnsureRoleAsync(workspaceId, userId, allowedRoles);
    }

    public async Task<UserWorkspace> EnsureSprintRoleAsync(int sprintId, string userId, params UserRole[] allowedRoles)
    {
        var workspaceId = await GetWorkspaceIdForSprintAsync(sprintId);
        return await EnsureRoleAsync(workspaceId, userId, allowedRoles);
    }

    public async Task<UserWorkspace> EnsureTaskRoleAsync(int taskId, string userId, params UserRole[] allowedRoles)
    {
        var workspaceId = await GetWorkspaceIdForTaskAsync(taskId);
        return await EnsureRoleAsync(workspaceId, userId, allowedRoles);
    }

    public async Task<int> GetWorkspaceIdForProjectAsync(int projectId)
    {
        var workspaceId = await _context.Projects
            .Where(p => p.Id == projectId)
            .Select(p => (int?)p.WorkspaceId)
            .FirstOrDefaultAsync();

        return workspaceId ?? throw new KeyNotFoundException($"Project with id {projectId} not found.");
    }

    public async Task<int> GetWorkspaceIdForSprintAsync(int sprintId)
    {
        var workspaceId = await _context.Sprints
            .Where(s => s.Id == sprintId)
            .Select(s => (int?)s.Project.WorkspaceId)
            .FirstOrDefaultAsync();

        return workspaceId ?? throw new KeyNotFoundException($"Sprint with id {sprintId} not found.");
    }

    public async Task<int> GetWorkspaceIdForTaskAsync(int taskId)
    {
        var workspaceId = await _context.ProjectTasks
            .Where(t => t.Id == taskId)
            .Select(t => (int?)t.Sprint!.Project.WorkspaceId)
            .FirstOrDefaultAsync();

        return workspaceId ?? throw new KeyNotFoundException($"Task with id {taskId} not found.");
    }

    public async Task<bool> IsTaskAssigneeAsync(int taskId, string userId)
    {
        return await _context.UserTasks
            .AnyAsync(ut => ut.ProjectTaskId == taskId &&
                            ut.AppUserId == userId &&
                            !ut.IsDeleted);
    }
}
