using AgileFlow.Application.Interfaces;
using AgileFlow.Infrastructure.Persistence.Data;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgileFlow.Infrastructure.Services;

/// <summary>
/// Background hosted service that runs every hour and sends a single due-date
/// reminder email per (task, assignee, due-date) combination.
/// Deduplication is enforced via <see cref="IEmailNotificationLogRepository"/>.
/// </summary>
public sealed class DueDateReminderWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DueDateReminderWorker> _logger;

    public DueDateReminderWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<DueDateReminderWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DueDateReminderWorker started.");

        // Run once at startup, then on the interval
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unhandled error in DueDateReminderWorker tick.");
            }

            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("DueDateReminderWorker stopped.");
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        _logger.LogDebug("DueDateReminderWorker tick started.");

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AgileFlowDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationEmailService>();

        var now = DateTime.UtcNow;
        var windowEnd = now.Add(ReminderWindow);

        // Find all active task-assignee pairs whose task is due within the next 24 hours
        var dueSoonAssignments = await db.UserTasks
            .IgnoreQueryFilters()
            .Where(ut =>
                !ut.IsDeleted &&
                ut.ProjectTask!.DueDate >= now &&
                ut.ProjectTask.DueDate <= windowEnd &&
                ut.ProjectTask.Status != ProjectTaskStatus.Done &&
                !ut.ProjectTask.IsDeleted)
            .Select(ut => new
            {
                ut.AppUserId,
                ut.ProjectTask!.Id,
                ut.ProjectTask.Title,
                ut.ProjectTask.DueDate,
            })
            .ToListAsync(ct);

        _logger.LogDebug("Found {Count} due-soon assignments to evaluate.", dueSoonAssignments.Count);

        foreach (var assignment in dueSoonAssignments)
        {
            var deduplicationKey =
                $"due:{assignment.Id}:{assignment.AppUserId}:{assignment.DueDate:yyyy-MM-dd}";

            // NotificationEmailService handles the deduplication-key check internally
            await notificationService.SendDueDateReminderAsync(
                userId: assignment.AppUserId,
                taskId: assignment.Id,
                taskTitle: assignment.Title,
                dueDate: assignment.DueDate,
                deduplicationKey: deduplicationKey,
                cancellationToken: ct);
        }

        _logger.LogDebug("DueDateReminderWorker tick finished.");
    }
}
