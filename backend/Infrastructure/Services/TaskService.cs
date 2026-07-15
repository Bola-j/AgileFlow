using AgileFlow.Domain.Entities;
using Application.DTOs.Tasks;
using Application.Interfaces;
using AgileFlow.Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IWorkspaceAuthorizationService _authorizationService;
    private readonly IMapper _mapper;
    private readonly INotificationEmailService _notificationEmail;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        ITaskRepository taskRepository,
        IWorkspaceAuthorizationService authorizationService,
        IMapper mapper,
        INotificationEmailService notificationEmail,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
        _mapper = mapper;
        _notificationEmail = notificationEmail;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskSummaryResponse>> GetBySprintAsync(int sprintId, string userId)
    {
        await _authorizationService.EnsureSprintMemberAsync(sprintId, userId);

        var tasks = await _taskRepository.GetBySprintIdAsync(sprintId);
        return _mapper.Map<IEnumerable<TaskSummaryResponse>>(tasks);
    }

    public async Task<TaskDetailResponse?> GetByIdAsync(int id, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await _authorizationService.EnsureTaskMemberAsync(id, userId);

        return _mapper.Map<TaskDetailResponse>(task);
    }

    public async Task<TaskDetailResponse> CreateAsync(int sprintId, CreateTaskRequest request, string userId)
    {
        await _authorizationService.EnsureSprintRoleAsync(sprintId, userId, UserRole.Admin, UserRole.TeamLead);

        var sprint = await _taskRepository.GetSprintByIdAsync(sprintId)
            ?? throw new KeyNotFoundException($"Sprint with id {sprintId} not found.");

        var column = await _taskRepository.GetColumnByIdAsync(request.ColumnId)
            ?? throw new KeyNotFoundException($"Board column with id {request.ColumnId} not found.");

        if (column.Board.ProjectId != sprint.ProjectId)
            throw new InvalidOperationException("The selected column does not belong to the sprint project.");

        if (request.DueDate == default)
            throw new InvalidOperationException("DueDate is required.");

        var task = new ProjectTask(
            title: request.Title,
            status: request.Status,
            priority: request.Priority,
            columnId: request.ColumnId,
            dueDate: request.DueDate,
            description: request.Description ?? string.Empty,
            sprintId: sprintId);

        await _taskRepository.AddAsync(task);

        foreach (var assigneeId in request.AssigneeUserIds.Distinct())
        {
            await AssignExistingTaskUserAsync(task.Id, sprint.Project.WorkspaceId, assigneeId);
        }

        var created = await _taskRepository.GetByIdAsync(task.Id);
        return _mapper.Map<TaskDetailResponse>(created!);
    }

    public async Task<TaskDetailResponse?> UpdateAsync(int id, UpdateTaskRequest request, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await _authorizationService.EnsureTaskRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);

        if (request.DueDate == default)
            throw new InvalidOperationException("DueDate is required.");

        var logs = new List<TaskActivityLog>();

        if (task.Title != request.Title)
        {
            logs.Add(new TaskActivityLog("Title", id, userId, request.Title, task.Title));
            task.UpdateTitle(request.Title);
        }

        if (task.Description != request.Description)
        {
            logs.Add(new TaskActivityLog("Description", id, userId, request.Description ?? string.Empty, task.Description));
            task.UpdateDescription(request.Description ?? string.Empty);
        }

        if (task.Status != request.Status)
        {
            logs.Add(new TaskActivityLog("Status", id, userId, request.Status.ToString(), task.Status.ToString()));
            task.UpdateStatus(request.Status);
        }

        if (task.Priority != request.Priority)
        {
            logs.Add(new TaskActivityLog("Priority", id, userId, request.Priority.ToString(), task.Priority.ToString()));
            task.UpdatePriority(request.Priority);
        }

        if (task.DueDate != request.DueDate)
        {
            logs.Add(new TaskActivityLog("DueDate", id, userId, request.DueDate.ToString("yyyy-MM-dd"), task.DueDate.ToString("yyyy-MM-dd")));
            task.UpdateDueDate(request.DueDate);
        }

        // Notify on ApprovalStatus change
        if (request.ApprovalStatus.HasValue && task.ApprovalStatus != request.ApprovalStatus.Value)
        {
            var oldApproval = task.ApprovalStatus;
            logs.Add(new TaskActivityLog("ApprovalStatus", id, userId, request.ApprovalStatus.Value.ToString(),
                oldApproval?.ToString() ?? string.Empty));
            task.UpdateApprovalStatus(request.ApprovalStatus.Value);

            // Fire review-decision notification to each assignee
            var updatedForNotif = await _taskRepository.GetByIdAsync(id);
            if (updatedForNotif is not null)
            {
                var isApproved = request.ApprovalStatus.Value == ProjectTaskApprovalStatus.Approved;
                foreach (var ut in updatedForNotif.UserTasks.Where(x => !x.IsDeleted))
                {
                    await TrySendNotificationAsync(() =>
                        _notificationEmail.SendTaskReviewDecisionAsync(
                            ut.AppUserId, id, task.Title, isApproved));
                }
            }
        }

        await _taskRepository.UpdateAsync(task);
        foreach (var log in logs)
        {
            await _taskRepository.AddActivityLogAsync(log);
        }

        var updated = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDetailResponse>(updated!);
    }

    public async Task<TaskDetailResponse?> UpdateStatusAsync(int id, UpdateTaskStatusRequest request, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await EnsureCanProgressTaskAsync(id, userId);

        if (task.Status != request.Status)
        {
            var oldStatus = task.Status.ToString();
            task.UpdateStatus(request.Status);
            await _taskRepository.UpdateAsync(task);
            var log = new TaskActivityLog("Status", id, userId, request.Status.ToString(), oldStatus);
            await _taskRepository.AddActivityLogAsync(log);
        }
        var updated = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDetailResponse>(updated!);
    }

    public async Task<TaskDetailResponse?> MoveAsync(int id, MoveTaskRequest request, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await EnsureCanProgressTaskAsync(id, userId);

        var column = await _taskRepository.GetColumnByIdAsync(request.ColumnId)
            ?? throw new KeyNotFoundException($"Board column with id {request.ColumnId} not found.");

        if (column.Board.ProjectId != task.Sprint!.ProjectId)
            throw new InvalidOperationException("The selected column does not belong to the task project.");

        if (task.ColumnId != request.ColumnId)
        {
            var oldColumnId = task.ColumnId.ToString();
            task.UpdateColumn(request.ColumnId);
            await _taskRepository.UpdateAsync(task);
            var log = new TaskActivityLog("ColumnId", id, userId, request.ColumnId.ToString(), oldColumnId);
            await _taskRepository.AddActivityLogAsync(log);
        }

        var updated = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDetailResponse>(updated!);
    }

    public async Task<TaskDetailResponse?> AssignUserAsync(int id, AssignTaskRequest request, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await _authorizationService.EnsureTaskRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);
        await AssignExistingTaskUserAsync(id, task.Sprint!.Project.WorkspaceId, request.UserId);

        // Notify the newly assigned user
        await TrySendNotificationAsync(() =>
            _notificationEmail.SendTaskAssignedAsync(request.UserId, id, task.Title));

        var updated = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDetailResponse>(updated!);
    }

    public async Task<TaskDetailResponse?> UnassignUserAsync(int id, string assigneeUserId, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await _authorizationService.EnsureTaskRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);

        var assignment = await _taskRepository.GetAssignmentAsync(id, assigneeUserId);
        if (assignment is not null)
        {
            assignment.Delete();
            await _taskRepository.UpdateAssignmentAsync(assignment);
        }

        var updated = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDetailResponse>(updated!);
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return false;

        await _authorizationService.EnsureTaskRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);
        await _taskRepository.DeleteAsync(task);

        return true;
    }

    private async Task EnsureCanProgressTaskAsync(int taskId, string userId)
    {
        await _authorizationService.EnsureTaskMemberAsync(taskId, userId);

        if (await _authorizationService.IsTaskAssigneeAsync(taskId, userId))
            return;

        await _authorizationService.EnsureTaskRoleAsync(taskId, userId, UserRole.Admin, UserRole.TeamLead);
    }

    private async Task AssignExistingTaskUserAsync(int taskId, int workspaceId, string assigneeUserId)
    {
        await _authorizationService.EnsureMemberAsync(workspaceId, assigneeUserId);

        var assignment = await _taskRepository.GetAssignmentAsync(taskId, assigneeUserId, includeDeleted: true);
        if (assignment is null)
        {
            await _taskRepository.AddAssignmentAsync(new UserTask(assigneeUserId, taskId));
            return;
        }

        if (assignment.IsDeleted)
        {
            assignment.Restore();
            await _taskRepository.UpdateAssignmentAsync(assignment);
        }
    }

    private async Task<bool> HasCircularDependencyAsync(int currentTaskId, int targetDependencyId)
    {
        if (currentTaskId == targetDependencyId)
            return true;
        var nextDependencies = await _taskRepository.GetDependedTaskIdsAsync(targetDependencyId);
        foreach (var nextId in nextDependencies)
        {
            if (await HasCircularDependencyAsync(currentTaskId, nextId))
                return true;
        }
        return false;
    }

    public async Task<bool> AddDependencyAsync(int taskId, int dependencyTaskId, string userId)
    {
        await _authorizationService.EnsureTaskRoleAsync(taskId, userId, UserRole.Admin, UserRole.TeamLead);
        await _authorizationService.EnsureTaskMemberAsync(dependencyTaskId, userId);

        if (taskId == dependencyTaskId)
            throw new InvalidOperationException("A task cannot depend on itself.");

        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException($"Task with id {taskId} not found.");

        var dependencyTask = await _taskRepository.GetByIdAsync(dependencyTaskId)
            ?? throw new KeyNotFoundException($"Dependency task with id {dependencyTaskId} not found.");

        if (task.Sprint!.ProjectId != dependencyTask.Sprint!.ProjectId)
            throw new InvalidOperationException("Both tasks must belong to the same project.");

        var existingDependency = await _taskRepository.GetDependencyAsync(taskId, dependencyTaskId);
        if (existingDependency is not null)
            throw new InvalidOperationException("This dependency already exists.");

        if (await HasCircularDependencyAsync(taskId, dependencyTaskId))
            throw new InvalidOperationException("Circular dependency detected. This assignment is rejected.");

        var dependency = new TaskDependent(taskId, dependencyTaskId);
        await _taskRepository.AddDependencyAsync(dependency);

        var log = new TaskActivityLog("DependencyAdded", taskId, userId, $"Task #{dependencyTaskId}", string.Empty);
        await _taskRepository.AddActivityLogAsync(log);

        return true;
    }

    public async Task<bool> RemoveDependencyAsync(int taskId, int dependencyTaskId, string userId)
    {
        await _authorizationService.EnsureTaskRoleAsync(taskId, userId, UserRole.Admin, UserRole.TeamLead);
        var dependency = await _taskRepository.GetDependencyAsync(taskId, dependencyTaskId)
            ?? throw new KeyNotFoundException("The specified dependency does not exist.");
        await _taskRepository.RemoveDependencyAsync(dependency);
        var log = new TaskActivityLog("DependencyRemoved", taskId, userId, string.Empty, $"Task #{dependencyTaskId}");
        await _taskRepository.AddActivityLogAsync(log);
        return true;
    }

    public async Task<IEnumerable<TaskActivityLogResponse>> GetActivityLogsAsync(int taskId, string userId)
    {
        await _authorizationService.EnsureTaskMemberAsync(taskId, userId);
        var logs = await _taskRepository.GetActivityLogsByTaskIdAsync(taskId);
        return _mapper.Map<IEnumerable<TaskActivityLogResponse>>(logs);
    }

    /// <summary>
    /// Invokes <paramref name="notificationTask"/> and swallows any exception,
    /// logging it so the primary business action is never rolled back by email failures.
    /// </summary>
    private async Task TrySendNotificationAsync(Func<Task> notificationTask)
    {
        try
        {
            await notificationTask();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email notification failed (non-fatal).");
        }
    }
}
