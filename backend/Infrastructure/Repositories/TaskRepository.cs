using AgileFlow.Domain.Entities;
using AgileFlow.Infrastructure.Persistence.Data;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AgileFlowDbContext _context;

    public TaskRepository(AgileFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectTask>> GetBySprintIdAsync(int sprintId)
    {
        return await TasksWithDetails()
            .Where(t => t.SprintId == sprintId)
            .ToListAsync();
    }

    public async Task<ProjectTask?> GetByIdAsync(int id)
    {
        return await TasksWithDetails()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Sprint?> GetSprintByIdAsync(int sprintId)
    {
        return await _context.Sprints
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.Id == sprintId);
    }

    public async Task<BoardColumn?> GetColumnByIdAsync(int columnId)
    {
        return await _context.BoardColumns
            .Include(c => c.Board)
            .FirstOrDefaultAsync(c => c.Id == columnId);
    }

    public async Task<ProjectTask> AddAsync(ProjectTask task)
    {
        await _context.ProjectTasks.AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(ProjectTask task)
    {
        _context.ProjectTasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ProjectTask task)
    {
        task.Delete();
        _context.ProjectTasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task<UserTask?> GetAssignmentAsync(int taskId, string userId, bool includeDeleted = false)
    {
        var query = includeDeleted ? _context.UserTasks.IgnoreQueryFilters() : _context.UserTasks;

        return await query.FirstOrDefaultAsync(ut => ut.ProjectTaskId == taskId &&
                                                     ut.AppUserId == userId);
    }

    public async Task AddAssignmentAsync(UserTask assignment)
    {
        await _context.UserTasks.AddAsync(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAssignmentAsync(UserTask assignment)
    {
        _context.UserTasks.Update(assignment);
        await _context.SaveChangesAsync();
    }

    private IQueryable<ProjectTask> TasksWithDetails()
    {
        return _context.ProjectTasks
            .AsSplitQuery()
            .Include(t => t.UserTasks.Where(ut => !ut.IsDeleted))
                .ThenInclude(ut => ut.AppUser)
            .Include(t => t.Sprint)
                .ThenInclude(s => s!.Project)
            .Include(t => t.Column)
                .ThenInclude(c => c.Board)
            .Include(t => t.TaskDependents)
                .ThenInclude(td => td.DependedTask)
            .Include(t => t.Commits.Where(commit => !commit.IsDeleted))
                .ThenInclude(commit => commit.AppUser)
            .Include(t => t.Comments.Where(comment => !comment.IsDeleted))
                .ThenInclude(comment => comment.AppUser);
    }

    public async Task<TaskDependent?> GetDependencyAsync(int taskId, int dependedTaskId)
    {
        return await _context.TaskDependents
            .FirstOrDefaultAsync(td => td.TaskId == taskId && td.DependedTaskId == dependedTaskId);
    }

    public async Task AddDependencyAsync(TaskDependent dependency)
    {
        await _context.TaskDependents.AddAsync(dependency);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveDependencyAsync(TaskDependent dependency)
    {
        _context.TaskDependents.Remove(dependency);
        await _context.SaveChangesAsync();
    }

    public async Task<List<int>> GetDependedTaskIdsAsync(int taskId)
    {
        return await _context.TaskDependents
            .Where(td => td.TaskId == taskId)
            .Select(td => td.DependedTaskId)
            .ToListAsync();
    }

    public async Task<List<string>> GetWorkspaceReviewerUserIdsForTaskAsync(int taskId)
    {
        var workspaceId = await _context.ProjectTasks
            .Where(task => task.Id == taskId)
            .Select(task => task.Sprint!.Project.WorkspaceId)
            .FirstOrDefaultAsync();

        if (workspaceId == 0)
            return new List<string>();

        return await _context.UserWorkspaces
            .Where(membership =>
                membership.WorkspaceId == workspaceId &&
                !membership.IsDeleted &&
                (membership.Role == UserRole.Admin || membership.Role == UserRole.TeamLead))
            .Select(membership => membership.AppUserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task AddActivityLogAsync(TaskActivityLog log)
    {
        await _context.TaskActivityLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskActivityLog>> GetActivityLogsByTaskIdAsync(int taskId)
    {
        return await _context.TaskActivityLogs
            .Include(l => l.AppUser)
            .Where(l => l.ProjectTaskId == taskId)
            .OrderByDescending(l => l.Id)
            .ToListAsync();
    }

    public async Task<Commit?> GetLatestCommitAsync(int taskId)
    {
        return await _context.Commits
            .Where(commit => commit.ProjectTaskId == taskId && !commit.IsDeleted)
            .OrderByDescending(commit => commit.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddCommitAsync(Commit commit)
    {
        await _context.Commits.AddAsync(commit);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCommitAsync(Commit commit)
    {
        _context.Commits.Update(commit);
        await _context.SaveChangesAsync();
    }

    public async Task AddCommentAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }
}
