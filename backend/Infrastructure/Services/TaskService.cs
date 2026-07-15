using AgileFlow.Domain.Entities;
using Application.DTOs.Tasks;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IWorkspaceAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public TaskService(
        ITaskRepository taskRepository,
        IWorkspaceAuthorizationService authorizationService,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
        _mapper = mapper;
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

        if (column.IsDeleted)
            throw new InvalidOperationException("The selected column is deleted.");

        if (column.Board.ProjectId != sprint.ProjectId)
            throw new InvalidOperationException("The selected column does not belong to the sprint project.");

        var initialStatus = StatusForColumn(column);
        if (initialStatus == ProjectTaskStatus.Done)
            throw new InvalidOperationException("Tasks cannot be created directly in the Done column. Submit and approve work before moving it to Done.");

        if (request.DueDate == default)
            throw new InvalidOperationException("DueDate is required.");

        ValidateTaskDueDate(request.DueDate, sprint.StartDate, sprint.EndDate, sprint.Project.EndDate);

        var task = new ProjectTask(
            title: request.Title,
            status: initialStatus,
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

        var membership = await _authorizationService.EnsureMemberAsync(task.Sprint!.Project.WorkspaceId, userId);
        var isManager = membership.Role is UserRole.Admin or UserRole.TeamLead;
        var isAssignee = await _authorizationService.IsTaskAssigneeAsync(id, userId);

        if (!isManager && !isAssignee)
            throw new UnauthorizedAccessException("Only task assignees, admins, or team leads can update this task.");

        if (request.DueDate == default)
            throw new InvalidOperationException("DueDate is required.");

        ValidateTaskDueDate(request.DueDate, task.Sprint!.StartDate, task.Sprint.EndDate, task.Sprint.Project.EndDate);

        var logs = new List<TaskActivityLog>();

        if (task.Title != request.Title)
        {
            logs.Add(new TaskActivityLog("Title", id, userId, task.Title, request.Title));
            task.UpdateTitle(request.Title);
        }

        if (task.Description != request.Description)
        {
            logs.Add(new TaskActivityLog("Description", id, userId, task.Description, request.Description ?? string.Empty));
            task.UpdateDescription(request.Description ?? string.Empty);
        }

        if (task.Status != request.Status)
        {
            throw new InvalidOperationException("Task status is controlled by board movement and review submission.");
        }

        if (task.Priority != request.Priority)
        {
            logs.Add(new TaskActivityLog("Priority", id, userId, task.Priority.ToString(), request.Priority.ToString()));
            task.UpdatePriority(request.Priority);
        }

        if (task.DueDate != request.DueDate)
        {
            logs.Add(new TaskActivityLog("DueDate", id, userId, task.DueDate.ToString("yyyy-MM-dd"), request.DueDate.ToString("yyyy-MM-dd")));
            task.UpdateDueDate(request.DueDate);
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

        await _authorizationService.EnsureTaskRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);

        if (task.Status != request.Status)
        {
            if (request.Status == ProjectTaskStatus.Done && task.ApprovalStatus != ProjectTaskApprovalStatus.Approved)
                throw new InvalidOperationException("Task status can become Done only after review approval.");

            if (task.ApprovalStatus == ProjectTaskApprovalStatus.Pending)
                throw new InvalidOperationException("Pending review tasks must be approved or rejected before changing status.");

            var oldStatus = task.Status.ToString();
            task.UpdateStatus(request.Status);
            if (request.Status != ProjectTaskStatus.Done && task.ApprovalStatus == ProjectTaskApprovalStatus.Approved)
                task.ClearApprovalStatus();

            await _taskRepository.UpdateAsync(task);
            var log = new TaskActivityLog("Status", id, userId, oldStatus, request.Status.ToString());
            await _taskRepository.AddActivityLogAsync(log);
        }
        var updated = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDetailResponse>(updated!);
    }

    public async Task<TaskDetailResponse?> SubmitAsync(int id, SubmitTaskRequest request, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await _authorizationService.EnsureTaskMemberAsync(id, userId);
        if (!await _authorizationService.IsTaskAssigneeAsync(id, userId))
            throw new UnauthorizedAccessException("Only an assigned developer can submit this task for review.");

        var commitHash = request.CommitHash.Trim();
        if (string.IsNullOrWhiteSpace(commitHash))
            throw new InvalidOperationException("Commit hash is required.");

        EnsureDependenciesCompleted(task);

        if (task.ApprovalStatus == ProjectTaskApprovalStatus.Pending)
            throw new InvalidOperationException("This task is already pending review.");

        var logs = new List<TaskActivityLog>();
        if (task.ApprovalStatus != ProjectTaskApprovalStatus.Pending)
        {
            logs.Add(new TaskActivityLog("ApprovalStatus", id, userId, task.ApprovalStatus?.ToString() ?? string.Empty, ProjectTaskApprovalStatus.Pending.ToString()));
            task.UpdateApprovalStatus(ProjectTaskApprovalStatus.Pending);
        }

        await _taskRepository.UpdateAsync(task);
        await _taskRepository.AddCommitAsync(new Commit(
            message: "Task submitted for review",
            branchName: string.Empty,
            commitHash: commitHash,
            url: string.Empty,
            status: CommitStatus.Pending,
            projectTaskId: id,
            appUserId: userId));

        logs.Add(new TaskActivityLog("CommitSubmitted", id, userId, string.Empty, commitHash));
        foreach (var log in logs)
        {
            await _taskRepository.AddActivityLogAsync(log);
        }

        var updated = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDetailResponse>(updated!);
    }

    public async Task<TaskDetailResponse?> ReviewAsync(int id, ReviewTaskRequest request, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await _authorizationService.EnsureTaskRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);

        if (request.ApprovalStatus is not ProjectTaskApprovalStatus.Approved and not ProjectTaskApprovalStatus.Rejected)
            throw new InvalidOperationException("Review must approve or reject the task.");

        var comment = request.Comment.Trim();
        if (string.IsNullOrWhiteSpace(comment))
            throw new InvalidOperationException("Review comment is required.");

        if (task.ApprovalStatus != ProjectTaskApprovalStatus.Pending)
            throw new InvalidOperationException("Only pending tasks can be reviewed.");

        var latestCommit = await _taskRepository.GetLatestCommitAsync(id)
            ?? throw new InvalidOperationException("Task must have a submitted commit before review.");

        if (request.ApprovalStatus == ProjectTaskApprovalStatus.Approved)
            EnsureDependenciesCompleted(task);

        var oldApproval = task.ApprovalStatus?.ToString() ?? string.Empty;
        var oldStatus = task.Status.ToString();
        task.UpdateApprovalStatus(request.ApprovalStatus);

        if (request.ApprovalStatus == ProjectTaskApprovalStatus.Approved)
        {
            task.UpdateStatus(ProjectTaskStatus.Done);
        }
        else if (task.Status == ProjectTaskStatus.Done)
        {
            task.UpdateStatus(ProjectTaskStatus.InProgress);
        }

        latestCommit.UpdateStatus(request.ApprovalStatus == ProjectTaskApprovalStatus.Approved
            ? CommitStatus.Merged
            : CommitStatus.Rejected);

        await _taskRepository.UpdateAsync(task);
        await _taskRepository.UpdateCommitAsync(latestCommit);
        await _taskRepository.AddCommentAsync(new Comment(comment, id, userId));
        await _taskRepository.AddActivityLogAsync(new TaskActivityLog("ApprovalStatus", id, userId, oldApproval, request.ApprovalStatus.ToString()));
        if (oldStatus != task.Status.ToString())
            await _taskRepository.AddActivityLogAsync(new TaskActivityLog("Status", id, userId, oldStatus, task.Status.ToString()));
        await _taskRepository.AddActivityLogAsync(new TaskActivityLog("ReviewComment", id, userId, string.Empty, comment));

        var updated = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDetailResponse>(updated!);
    }

    public async Task<TaskDetailResponse?> MoveAsync(int id, MoveTaskRequest request, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await _authorizationService.EnsureTaskRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);

        var column = await _taskRepository.GetColumnByIdAsync(request.ColumnId)
            ?? throw new KeyNotFoundException($"Board column with id {request.ColumnId} not found.");

        if (column.IsDeleted)
            throw new InvalidOperationException("The selected column is deleted.");

        if (column.Board.ProjectId != task.Sprint!.ProjectId)
            throw new InvalidOperationException("The selected column does not belong to the task project.");

        var nextStatus = StatusForColumn(column);
        if (nextStatus == ProjectTaskStatus.Done &&
            (task.Status != ProjectTaskStatus.Done || task.ApprovalStatus != ProjectTaskApprovalStatus.Approved))
        {
            throw new InvalidOperationException("Task can move to the Done column only after it is completed and approved.");
        }

        if (nextStatus == ProjectTaskStatus.Done)
            EnsureDependenciesCompleted(task);

        if (task.ColumnId != request.ColumnId)
        {
            var oldColumnId = task.ColumnId.ToString();
            var oldStatus = task.Status.ToString();
            var oldApproval = task.ApprovalStatus?.ToString() ?? string.Empty;

            task.UpdateColumn(request.ColumnId);
            if (nextStatus != task.Status)
                task.UpdateStatus(nextStatus);

            if (nextStatus != ProjectTaskStatus.Done && task.ApprovalStatus is ProjectTaskApprovalStatus.Pending or ProjectTaskApprovalStatus.Approved)
                task.ClearApprovalStatus();

            await _taskRepository.UpdateAsync(task);
            var log = new TaskActivityLog("ColumnId", id, userId, oldColumnId, request.ColumnId.ToString());
            await _taskRepository.AddActivityLogAsync(log);
            if (oldStatus != task.Status.ToString())
                await _taskRepository.AddActivityLogAsync(new TaskActivityLog("Status", id, userId, oldStatus, task.Status.ToString()));
            if (oldApproval != (task.ApprovalStatus?.ToString() ?? string.Empty))
                await _taskRepository.AddActivityLogAsync(new TaskActivityLog("ApprovalStatus", id, userId, oldApproval, task.ApprovalStatus?.ToString() ?? string.Empty));
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

    private static void ValidateTaskDueDate(DateTime dueDate, DateTime sprintStartDate, DateTime sprintEndDate, DateTime projectEndDate)
    {
        var due = dueDate.Date;
        if (due < sprintStartDate.Date)
            throw new InvalidOperationException("Task DueDate cannot be before the sprint StartDate.");

        if (due > sprintEndDate.Date)
            throw new InvalidOperationException("Task DueDate cannot be after the sprint EndDate.");

        if (due > projectEndDate.Date)
            throw new InvalidOperationException("Task DueDate cannot be after the project EndDate.");
    }

    private static ProjectTaskStatus StatusForColumn(BoardColumn column)
    {
        var normalized = column.Name.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
        if (normalized.Equals("Done", StringComparison.OrdinalIgnoreCase))
            return ProjectTaskStatus.Done;

        if (normalized.Equals("InProgress", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("Doing", StringComparison.OrdinalIgnoreCase))
        {
            return ProjectTaskStatus.InProgress;
        }

        if (normalized.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            return ProjectTaskStatus.Cancelled;

        return ProjectTaskStatus.Todo;
    }

    private static void EnsureDependenciesCompleted(ProjectTask task)
    {
        var incompleteDependencies = task.TaskDependents
            .Where(dependency =>
                dependency.DependedTask.Status != ProjectTaskStatus.Done ||
                dependency.DependedTask.ApprovalStatus != ProjectTaskApprovalStatus.Approved)
            .Select(dependency => dependency.DependedTask.Title)
            .ToList();

        if (incompleteDependencies.Count > 0)
            throw new InvalidOperationException($"Complete and approve dependencies first: {string.Join(", ", incompleteDependencies)}.");
    }

    private async Task<bool> HasCircularDependencyAsync(
        int currentTaskId,
        int targetDependencyId,
        HashSet<int>? visitedTaskIds = null)
    {
        visitedTaskIds ??= new HashSet<int>();

        if (currentTaskId == targetDependencyId)
            return true;

        if (!visitedTaskIds.Add(targetDependencyId))
            return false;

        var nextDependencies = await _taskRepository.GetDependedTaskIdsAsync(targetDependencyId);
        foreach (var nextId in nextDependencies)
        {
            if (await HasCircularDependencyAsync(currentTaskId, nextId, visitedTaskIds))
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

        if (task.Status == ProjectTaskStatus.Done || task.ApprovalStatus == ProjectTaskApprovalStatus.Pending)
            throw new InvalidOperationException("Dependencies cannot be changed while a task is completed or pending review.");

        var existingDependency = await _taskRepository.GetDependencyAsync(taskId, dependencyTaskId);
        if (existingDependency is not null)
            throw new InvalidOperationException("This dependency already exists.");

        if (await HasCircularDependencyAsync(taskId, dependencyTaskId))
            throw new InvalidOperationException("Circular dependency detected. This assignment is rejected.");

        var dependency = new TaskDependent(taskId, dependencyTaskId);
        await _taskRepository.AddDependencyAsync(dependency);

        var log = new TaskActivityLog("DependencyAdded", taskId, userId, string.Empty, $"Task #{dependencyTaskId}");
        await _taskRepository.AddActivityLogAsync(log);

        return true;
    }

    public async Task<bool> RemoveDependencyAsync(int taskId, int dependencyTaskId, string userId)
    {
        await _authorizationService.EnsureTaskRoleAsync(taskId, userId, UserRole.Admin, UserRole.TeamLead);
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException($"Task with id {taskId} not found.");

        if (task.Status == ProjectTaskStatus.Done || task.ApprovalStatus == ProjectTaskApprovalStatus.Pending)
            throw new InvalidOperationException("Dependencies cannot be changed while a task is completed or pending review.");

        var dependency = await _taskRepository.GetDependencyAsync(taskId, dependencyTaskId)
            ?? throw new KeyNotFoundException("The specified dependency does not exist.");
        await _taskRepository.RemoveDependencyAsync(dependency);
        var log = new TaskActivityLog("DependencyRemoved", taskId, userId, $"Task #{dependencyTaskId}", string.Empty);
        await _taskRepository.AddActivityLogAsync(log);
        return true;
    }

    public async Task<IEnumerable<TaskActivityLogResponse>> GetActivityLogsAsync(int taskId, string userId)
    {
        await _authorizationService.EnsureTaskMemberAsync(taskId, userId);
        var logs = await _taskRepository.GetActivityLogsByTaskIdAsync(taskId);
        return _mapper.Map<IEnumerable<TaskActivityLogResponse>>(logs);
    }

}
