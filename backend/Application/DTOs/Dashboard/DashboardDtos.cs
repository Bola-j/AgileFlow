using Application.DTOs.Project;
using Application.DTOs.Sprint;
using Application.DTOs.Tasks;
using Application.DTOs.Workspace;

namespace Application.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    public List<WorkspaceSummaryResponse> Workspaces { get; set; } = new();
    public List<ProjectResponse> Projects { get; set; } = new();
    public List<SprintResponse> Sprints { get; set; } = new();
    public List<TaskSummaryResponse> Tasks { get; set; } = new();
    public List<TaskSummaryResponse> AssignedTasks { get; set; } = new();
}

public class MyTaskResponse : TaskSummaryResponse
{
    public int WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string SprintName { get; set; } = string.Empty;
    public List<WorkspaceMemberResponse> WorkspaceMembers { get; set; } = new();
}
