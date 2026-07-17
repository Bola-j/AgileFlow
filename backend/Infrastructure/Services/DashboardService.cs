using AgileFlow.Infrastructure.Persistence.Data;
using Application.DTOs.Dashboard;
using Application.DTOs.Project;
using Application.DTOs.Sprint;
using Application.DTOs.Tasks;
using Application.DTOs.Workspace;
using Application.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AgileFlowDbContext _context;
    private readonly IMapper _mapper;

    public DashboardService(AgileFlowDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(string userId)
    {
        var workspaceIds = await _context.UserWorkspaces
            .Where(member => member.AppUserId == userId && !member.IsDeleted && !member.Workspace.IsDeleted)
            .Select(member => member.WorkspaceId)
            .Distinct()
            .ToListAsync();

        var workspaces = await _context.Workspaces
            .Where(workspace => workspaceIds.Contains(workspace.Id) && !workspace.IsDeleted)
            .Include(workspace => workspace.Projects.Where(project => !project.IsDeleted))
            .Include(workspace => workspace.UserWorkspaces.Where(member => !member.IsDeleted))
            .OrderByDescending(workspace => workspace.CreatedAt)
            .ToListAsync();

        var projects = await _context.Projects
            .Where(project => workspaceIds.Contains(project.WorkspaceId) && !project.IsDeleted)
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync();

        var projectIds = projects.Select(project => project.Id).ToList();
        var sprints = await _context.Sprints
            .Where(sprint => projectIds.Contains(sprint.ProjectId) && !sprint.IsDeleted)
            .Include(sprint => sprint.Tasks.Where(task => !task.IsDeleted))
            .OrderByDescending(sprint => sprint.CreatedAt)
            .ToListAsync();

        var sprintIds = sprints.Select(sprint => sprint.Id).ToList();
        var tasks = await _context.ProjectTasks
            .Where(task => sprintIds.Contains(task.SprintId) && !task.IsDeleted)
            .Include(task => task.UserTasks.Where(assignment => !assignment.IsDeleted))
                .ThenInclude(assignment => assignment.AppUser)
            .Include(task => task.Sprint)
                .ThenInclude(sprint => sprint!.Project)
            .Include(task => task.Column)
                .ThenInclude(column => column.Board)
            .OrderByDescending(task => task.CreatedAt)
            .ToListAsync();

        var workspaceSummaries = _mapper.Map<List<WorkspaceSummaryResponse>>(workspaces);
        foreach (var summary in workspaceSummaries)
        {
            var workspace = workspaces.First(workspace => workspace.Id == summary.Id);
            summary.CurrentUserRole = workspace.UserWorkspaces
                .First(member => member.AppUserId == userId && !member.IsDeleted)
                .Role
                .ToString();
        }

        return new DashboardSummaryResponse
        {
            Workspaces = workspaceSummaries,
            Projects = _mapper.Map<List<ProjectResponse>>(projects),
            Sprints = _mapper.Map<List<SprintResponse>>(sprints),
            Tasks = _mapper.Map<List<TaskSummaryResponse>>(tasks),
            AssignedTasks = _mapper.Map<List<TaskSummaryResponse>>(tasks.Where(task =>
                task.UserTasks.Any(assignment => !assignment.IsDeleted && assignment.AppUserId == userId)))
        };
    }

    public async Task<List<MyTaskResponse>> GetMyTasksAsync(string userId)
    {
        var assignedTaskIds = await _context.UserTasks
            .Where(assignment => !assignment.IsDeleted && assignment.AppUserId == userId)
            .Select(assignment => assignment.ProjectTaskId)
            .ToListAsync();

        var tasks = await _context.ProjectTasks
            .Where(task => !task.IsDeleted &&
                           assignedTaskIds.Contains(task.Id) &&
                           !(task.Status == Domain.Enums.ProjectTaskStatus.Done &&
                             task.ApprovalStatus == Domain.Enums.ProjectTaskApprovalStatus.Approved))
            .Include(task => task.UserTasks.Where(assignment => !assignment.IsDeleted))
                .ThenInclude(assignment => assignment.AppUser)
            .Include(task => task.Sprint)
                .ThenInclude(sprint => sprint!.Project)
                    .ThenInclude(project => project.Workspace)
                        .ThenInclude(workspace => workspace.UserWorkspaces.Where(member => !member.IsDeleted))
                            .ThenInclude(member => member.AppUser)
            .Include(task => task.Column)
                .ThenInclude(column => column.Board)
            .OrderBy(task => task.DueDate)
            .ToListAsync();

        return tasks.Select(task =>
        {
            var response = _mapper.Map<MyTaskResponse>(task);
            var workspace = task.Sprint!.Project.Workspace;
            response.WorkspaceId = workspace.Id;
            response.WorkspaceName = workspace.Name;
            response.ProjectName = task.Sprint.Project.Name;
            response.SprintName = task.Sprint.Name;
            response.WorkspaceMembers = _mapper.Map<List<WorkspaceMemberResponse>>(workspace.UserWorkspaces.Where(member => !member.IsDeleted));
            return response;
        }).ToList();
    }
}
