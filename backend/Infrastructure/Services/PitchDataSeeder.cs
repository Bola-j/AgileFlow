using System.Text.Json;
using System.Text.Json.Serialization;
using AgileFlow.Domain.Entities;
using AgileFlow.Infrastructure.Persistence.Data;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgileFlow.Infrastructure.Services;

public sealed class PitchDataSeeder(
    AgileFlowDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<PitchDataSeeder> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task SeedAsync()
    {
        if (!configuration.GetValue("Seed:PitchData", false))
            return;

        var seed = await LoadSeedDataAsync();
        if (await context.Workspaces.AnyAsync(w => w.Name == seed.Workspace.Name))
        {
            logger.LogInformation("Pitch seed data already exists.");
            return;
        }

        logger.LogInformation("Creating pitch seed data from {SeedFile}.", ResolveSeedPath());

        await EnsureIdentityRolesAsync();
        var users = await CreateUsersAsync(seed);
        var workspace = new Workspace(seed.Workspace.Name, seed.Workspace.Description);

        context.Workspaces.Add(workspace);
        await context.SaveChangesAsync();

        context.UserWorkspaces.AddRange(seed.Users.Select(user =>
            new UserWorkspace(users[user.Key].Id, workspace.Id, user.WorkspaceRole)));

        foreach (var projectSeed in seed.Projects)
        {
            await CreateProjectAsync(workspace.Id, projectSeed, users);
        }

        context.Notifications.AddRange(seed.Notifications.Select(notification =>
            new Notification(
                notification.Title,
                notification.Message,
                notification.Type,
                users[notification.UserKey].Id)));

        await context.SaveChangesAsync();
        logger.LogInformation("Pitch seed data created. Demo users use password {Password}.", seed.DefaultPassword);
    }

    private async Task<PitchSeedData> LoadSeedDataAsync()
    {
        var path = ResolveSeedPath();
        if (!File.Exists(path))
            throw new FileNotFoundException("Pitch seed data file was not found.", path);

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<PitchSeedData>(stream, JsonOptions)
            ?? throw new InvalidOperationException("Pitch seed data file is empty or invalid.");
    }

    private string ResolveSeedPath()
    {
        var configuredPath = configuration["Seed:PitchDataPath"];
        if (!string.IsNullOrWhiteSpace(configuredPath))
            return Path.GetFullPath(configuredPath, environment.ContentRootPath);

        return Path.Combine(environment.ContentRootPath, "SeedData", "pitch-data.json");
    }

    private async Task EnsureIdentityRolesAsync()
    {
        foreach (var role in new[] { nameof(UserRole.Admin), nameof(UserRole.TeamLead), nameof(UserRole.Developer) })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task<Dictionary<string, AppUser>> CreateUsersAsync(PitchSeedData seed)
    {
        var users = new Dictionary<string, AppUser>(StringComparer.OrdinalIgnoreCase);
        foreach (var userSeed in seed.Users)
        {
            var user = await userManager.FindByEmailAsync(userSeed.Email);
            if (user is null)
            {
                user = new AppUser(
                    userSeed.FirstName,
                    userSeed.LastName,
                    userSeed.Email,
                    githubUsername: userSeed.GithubUsername)
                {
                    UserName = userSeed.Email,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(user, seed.DefaultPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to create pitch user {userSeed.Email}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }

            if (!await userManager.IsInRoleAsync(user, userSeed.IdentityRole))
                await userManager.AddToRoleAsync(user, userSeed.IdentityRole);

            users[userSeed.Key] = user;
        }

        return users;
    }

    private async Task CreateProjectAsync(int workspaceId, ProjectSeed projectSeed, IReadOnlyDictionary<string, AppUser> users)
    {
        var project = new Project(
            projectSeed.Name,
            projectSeed.Status,
            projectSeed.StartDate,
            workspaceId,
            projectSeed.Description,
            projectSeed.EndDate);

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var columns = await context.BoardColumns
            .Where(c => c.Board.ProjectId == project.Id)
            .ToDictionaryAsync(c => c.Name, StringComparer.OrdinalIgnoreCase);

        var sprintMap = new Dictionary<string, Sprint>(StringComparer.OrdinalIgnoreCase);
        foreach (var sprintSeed in projectSeed.Sprints)
        {
            var sprint = new Sprint(
                sprintSeed.Name,
                sprintSeed.Status,
                sprintSeed.StartDate,
                project.Id,
                sprintSeed.Goal,
                sprintSeed.EndDate);

            context.Sprints.Add(sprint);
            sprintMap[sprintSeed.Key] = sprint;
        }

        await context.SaveChangesAsync();

        var taskMap = new Dictionary<string, ProjectTask>(StringComparer.OrdinalIgnoreCase);
        foreach (var taskSeed in projectSeed.Tasks)
        {
            var task = new ProjectTask(
                taskSeed.Title,
                taskSeed.Status,
                taskSeed.Priority,
                columns[taskSeed.Column].Id,
                taskSeed.DueDate,
                taskSeed.Description,
                sprintMap[taskSeed.SprintKey].Id);

            if (taskSeed.ApprovalStatus is not null)
                task.UpdateApprovalStatus(taskSeed.ApprovalStatus.Value);

            context.ProjectTasks.Add(task);
            taskMap[taskSeed.Key] = task;
        }

        await context.SaveChangesAsync();

        context.UserTasks.AddRange(projectSeed.Tasks.Select(taskSeed =>
            new UserTask(users[taskSeed.AssigneeKey].Id, taskMap[taskSeed.Key].Id)));
        context.Comments.AddRange(projectSeed.Comments.Select(comment =>
            new Comment(comment.Content, taskMap[comment.TaskKey].Id, users[comment.UserKey].Id)));
        context.Commits.AddRange(projectSeed.Commits.Select(commit =>
            new Commit(
                commit.Message,
                commit.BranchName,
                commit.CommitHash,
                commit.Url,
                commit.Status,
                taskMap[commit.TaskKey].Id,
                users[commit.UserKey].Id)));
        context.TaskActivityLogs.AddRange(projectSeed.ActivityLogs.Select(log =>
            new TaskActivityLog(
                log.FieldChanged,
                taskMap[log.TaskKey].Id,
                users[log.UserKey].Id,
                log.OldValue,
                log.NewValue)));
    }

    private sealed record PitchSeedData(
        string DefaultPassword,
        WorkspaceSeed Workspace,
        List<UserSeed> Users,
        List<ProjectSeed> Projects,
        List<NotificationSeed> Notifications);

    private sealed record WorkspaceSeed(string Name, string Description);

    private sealed record UserSeed(
        string Key,
        string FirstName,
        string LastName,
        string Email,
        string GithubUsername,
        string IdentityRole,
        UserRole WorkspaceRole);

    private sealed record ProjectSeed(
        string Name,
        string Description,
        ProjectStatus Status,
        DateTime StartDate,
        DateTime EndDate,
        List<SprintSeed> Sprints,
        List<TaskSeed> Tasks,
        List<CommentSeed> Comments,
        List<CommitSeed> Commits,
        List<ActivityLogSeed> ActivityLogs);

    private sealed record SprintSeed(
        string Key,
        string Name,
        string Goal,
        SprintStatus Status,
        DateTime StartDate,
        DateTime EndDate);

    private sealed record TaskSeed(
        string Key,
        string SprintKey,
        string AssigneeKey,
        string Column,
        string Title,
        string Description,
        ProjectTaskStatus Status,
        ProjectTaskPriority Priority,
        ProjectTaskApprovalStatus? ApprovalStatus,
        DateTime DueDate);

    private sealed record CommentSeed(string TaskKey, string UserKey, string Content);

    private sealed record CommitSeed(
        string TaskKey,
        string UserKey,
        string Message,
        string BranchName,
        string CommitHash,
        string Url,
        CommitStatus Status);

    private sealed record ActivityLogSeed(
        string TaskKey,
        string UserKey,
        string FieldChanged,
        string OldValue,
        string NewValue);

    private sealed record NotificationSeed(
        string UserKey,
        string Title,
        string Message,
        NotificationType Type);
}
