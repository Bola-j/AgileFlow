namespace AgileFlow.Application.Interfaces;

/// <summary>
/// Sends workflow-triggered email notifications and writes an audit record to
/// <see cref="AgileFlow.Domain.Entities.EmailNotificationLog"/> regardless of outcome.
/// All methods swallow send errors so that a failed email never rolls back
/// the primary business action that triggered the notification.
/// </summary>
public interface INotificationEmailService
{
    /// <summary>
    /// Notifies a user that they have been added to a workspace.
    /// </summary>
    Task SendWorkspaceInviteAsync(
        string userId,
        string workspaceName,
        int workspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies a user that a task has been assigned to them.
    /// </summary>
    Task SendTaskAssignedAsync(
        string userId,
        int taskId,
        string taskTitle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies a user of a task review decision (approved or rejected).
    /// </summary>
    Task SendTaskReviewDecisionAsync(
        string userId,
        int taskId,
        string taskTitle,
        bool approved,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a due-date reminder. The <paramref name="deduplicationKey"/> is used
    /// to ensure only one reminder is sent per task/user/due-date window.
    /// </summary>
    Task SendDueDateReminderAsync(
        string userId,
        int taskId,
        string taskTitle,
        DateTime dueDate,
        string deduplicationKey,
        CancellationToken cancellationToken = default);
}
