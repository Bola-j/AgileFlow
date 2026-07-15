using AgileFlow.Domain.Entities;
using AgileFlow.Application.Interfaces;
using Application.DTOs.Sprint;
using Application.Interfaces;
using AutoMapper;
using Domain.Enums;

namespace Infrastructure.Services;

public class SprintService : ISprintService
{
    private readonly ISprintRepository _sprintRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkspaceAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public SprintService(
        ISprintRepository sprintRepository,
        IProjectRepository projectRepository,
        IWorkspaceAuthorizationService authorizationService,
        IMapper mapper)
    {
        _sprintRepository = sprintRepository;
        _projectRepository = projectRepository;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SprintResponse>> GetByProjectIdAsync(int projectId, string userId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
            throw new KeyNotFoundException($"Project with id {projectId} not found.");

        await _authorizationService.EnsureProjectMemberAsync(projectId, userId);

        var sprints = await _sprintRepository.GetByProjectIdAsync(projectId);
        return _mapper.Map<IEnumerable<SprintResponse>>(sprints);
    }

    public async Task<SprintResponse?> GetByIdAsync(int id, string userId)
    {
        var sprint = await _sprintRepository.GetByIdAsync(id);
        if (sprint is null) return null;

        await _authorizationService.EnsureSprintMemberAsync(id, userId);
        return _mapper.Map<SprintResponse>(sprint);
    }

    public async Task<SprintResponse> CreateAsync(int projectId, CreateSprintRequest request, string userId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
            throw new KeyNotFoundException($"Project with id {projectId} not found.");

        await _authorizationService.EnsureProjectRoleAsync(projectId, userId, UserRole.Admin, UserRole.TeamLead);

        ValidateDates(request.StartDate, request.EndDate, project.StartDate, project.EndDate);

        var sprint = new Sprint(
            name: request.Name,
            status: SprintStatus.Planning,
            startDate: request.StartDate,
            projectId: projectId,
            goal: request.Goal,
            endDate: request.EndDate);

        var created = await _sprintRepository.AddAsync(sprint);
        return _mapper.Map<SprintResponse>(created);
    }

    public async Task<SprintResponse?> UpdateAsync(int id, UpdateSprintRequest request, string userId)
    {
        var sprint = await _sprintRepository.GetByIdAsync(id);
        if (sprint is null) return null;

        await _authorizationService.EnsureSprintRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);

        var project = await _projectRepository.GetByIdAsync(sprint.ProjectId)
            ?? throw new KeyNotFoundException($"Project with id {sprint.ProjectId} not found.");

        ValidateDates(sprint.StartDate, request.EndDate, project.StartDate, project.EndDate);

        if (await _sprintRepository.HasTasksDueAfterAsync(id, request.EndDate))
            throw new InvalidOperationException("Sprint EndDate cannot be before any task DueDate.");

        sprint.UpdateName(request.Name);
        sprint.UpdateGoal(request.Goal);
        sprint.UpdateEndDate(request.EndDate);

        await _sprintRepository.UpdateAsync(sprint);

        var updated = await _sprintRepository.GetByIdAsync(id);
        return _mapper.Map<SprintResponse>(updated!);
    }

    public async Task<SprintResponse?> StartAsync(int id, string userId)
    {
        var sprint = await _sprintRepository.GetByIdAsync(id);
        if (sprint is null) return null;

        await _authorizationService.EnsureSprintRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);

        if (sprint.Status == SprintStatus.Completed || sprint.Status == SprintStatus.Cancelled)
            throw new InvalidOperationException("Only planning sprints can be started.");

        if (await _sprintRepository.HasActiveSprintInProjectAsync(sprint.ProjectId, excludeId: sprint.Id))
            throw new InvalidOperationException("Only one active sprint is allowed per project.");

        sprint.UpdateStatus(SprintStatus.Active);
        await _sprintRepository.UpdateAsync(sprint);

        var updated = await _sprintRepository.GetByIdAsync(id);
        return _mapper.Map<SprintResponse>(updated!);
    }

    public async Task<SprintResponse?> CompleteAsync(int id, string userId)
    {
        var sprint = await _sprintRepository.GetByIdAsync(id);
        if (sprint is null) return null;

        await _authorizationService.EnsureSprintRoleAsync(id, userId, UserRole.Admin, UserRole.TeamLead);

        if (sprint.Status != SprintStatus.Active)
            throw new InvalidOperationException("Only active sprints can be completed.");

        var incompleteTasks = sprint.Tasks
            .Where(task => task.Status != ProjectTaskStatus.Done ||
                           task.ApprovalStatus != ProjectTaskApprovalStatus.Approved)
            .Select(task => task.Title)
            .ToList();

        if (incompleteTasks.Count > 0)
            throw new InvalidOperationException($"Sprint can be completed only after all tasks are done and approved. Remaining tasks: {string.Join(", ", incompleteTasks)}.");

        sprint.UpdateStatus(SprintStatus.Completed);
        await _sprintRepository.UpdateAsync(sprint);

        var updated = await _sprintRepository.GetByIdAsync(id);
        return _mapper.Map<SprintResponse>(updated!);
    }

    public async Task<SprintProgressResponse?> GetProgressAsync(int id, string userId)
    {
        var sprint = await _sprintRepository.GetByIdAsync(id);
        if (sprint is null) return null;

        await _authorizationService.EnsureSprintMemberAsync(id, userId);

        var totalTasks = sprint.Tasks.Count;
        var completedTasks = sprint.Tasks.Count(t =>
            t.Status == ProjectTaskStatus.Done &&
            t.ApprovalStatus == ProjectTaskApprovalStatus.Approved);
        var percentage = totalTasks == 0
            ? 0m
            : Math.Round((decimal)completedTasks / totalTasks * 100m, 2);

        return new SprintProgressResponse
        {
            SprintId = sprint.Id,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            ProgressPercentage = percentage
        };
    }

    private static void ValidateDates(DateTime startDate, DateTime endDate, DateTime projectStartDate, DateTime projectEndDate)
    {
        if (startDate.Date < projectStartDate.Date)
            throw new InvalidOperationException("Sprint StartDate cannot be before the project StartDate.");

        if (endDate.Date <= startDate.Date)
            throw new InvalidOperationException("EndDate must be after StartDate.");

        if (endDate.Date > projectEndDate.Date)
            throw new InvalidOperationException("Sprint EndDate cannot be after the project EndDate.");
    }
}
