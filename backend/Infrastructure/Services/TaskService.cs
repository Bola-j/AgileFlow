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

        task.UpdateTitle(request.Title);
        task.UpdateDescription(request.Description ?? string.Empty);
        task.UpdateStatus(request.Status);
        task.UpdatePriority(request.Priority);
        task.UpdateDueDate(request.DueDate);

        await _taskRepository.UpdateAsync(task);

        var updated = await _taskRepository.GetByIdAsync(id);
        return _mapper.Map<TaskDetailResponse>(updated!);
    }

    public async Task<TaskDetailResponse?> UpdateStatusAsync(int id, UpdateTaskStatusRequest request, string userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null) return null;

        await EnsureCanProgressTaskAsync(id, userId);

        task.UpdateStatus(request.Status);
        await _taskRepository.UpdateAsync(task);

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

        task.UpdateColumn(request.ColumnId);
        await _taskRepository.UpdateAsync(task);

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

}
