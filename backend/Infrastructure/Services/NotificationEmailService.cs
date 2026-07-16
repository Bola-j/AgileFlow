using AgileFlow.Application.Interfaces;
using AgileFlow.Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AgileFlow.Infrastructure.Services;

/// <summary>
/// Sends workflow email notifications and records every attempt in
/// <see cref="EmailNotificationLog"/> regardless of outcome.
/// Failures are logged but never re-thrown, so the originating business action
/// is always committed successfully.
/// </summary>
public sealed class NotificationEmailService : INotificationEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly IEmailNotificationLogRepository _logRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<NotificationEmailService> _logger;

    public NotificationEmailService(
        IEmailSender emailSender,
        IEmailNotificationLogRepository logRepository,
        UserManager<AppUser> userManager,
        ILogger<NotificationEmailService> logger)
    {
        _emailSender = emailSender;
        _logRepository = logRepository;
        _userManager = userManager;
        _logger = logger;
    }

    // ── Public methods ────────────────────────────────────────────────────────

    public async Task SendWorkspaceInviteAsync(
        string userId,
        string workspaceName,
        int workspaceId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return;

        var deduplicationKey = $"workspace-invite:{workspaceId}:{userId}:{DateTime.UtcNow:yyyyMMddHH}";
        var subject = $"You have been added to workspace \"{workspaceName}\"";

        var html = $"""
            <h2>Welcome to {workspaceName}!</h2>
            <p>Hi {user.First_Name},</p>
            <p>You have been added to the workspace <strong>{workspaceName}</strong> on AgileFlow.</p>
            <p>Log in to start collaborating with your team.</p>
            <br/>
            <p>The AgileFlow Team</p>
            """;

        await SendAndLogAsync(
            recipientEmail: user.Email!,
            subject: subject,
            htmlBody: html,
            eventType: EmailEventType.WorkspaceInvite,
            deduplicationKey: deduplicationKey,
            relatedEntityId: workspaceId,
            relatedEntityType: "Workspace",
            cancellationToken: cancellationToken);
    }

    public async Task SendTaskAssignedAsync(
        string userId,
        int taskId,
        string taskTitle,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return;

        var deduplicationKey = $"assign:{taskId}:{userId}:{DateTime.UtcNow:yyyyMMddHHmm}";
        var subject = $"Task assigned to you: {taskTitle}";

        var html = $"""
            <h2>New Task Assignment</h2>
            <p>Hi {user.First_Name},</p>
            <p>You have been assigned to the task <strong>{taskTitle}</strong> (#{taskId}) on AgileFlow.</p>
            <p>Log in to view the task details and get started.</p>
            <br/>
            <p>The AgileFlow Team</p>
            """;

        await SendAndLogAsync(
            recipientEmail: user.Email!,
            subject: subject,
            htmlBody: html,
            eventType: EmailEventType.TaskAssigned,
            deduplicationKey: deduplicationKey,
            relatedEntityId: taskId,
            relatedEntityType: "ProjectTask",
            cancellationToken: cancellationToken);
    }

    public async Task SendTaskSubmittedForReviewAsync(
        string userId,
        int taskId,
        string taskTitle,
        string commitHash,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return;

        var deduplicationKey = $"submit-review:{taskId}:{userId}:{commitHash}";
        var subject = $"Task submitted for review: {taskTitle}";

        var html = $"""
            <h2>Task Submitted for Review</h2>
            <p>Hi {user.First_Name},</p>
            <p>The task <strong>{taskTitle}</strong> (#{taskId}) has been submitted for review.</p>
            <p>Commit hash: <code>{commitHash}</code></p>
            <p>Log in to approve or reject the submission.</p>
            <br/>
            <p>The AgileFlow Team</p>
            """;

        await SendAndLogAsync(
            recipientEmail: user.Email!,
            subject: subject,
            htmlBody: html,
            eventType: EmailEventType.TaskSubmittedForReview,
            deduplicationKey: deduplicationKey,
            relatedEntityId: taskId,
            relatedEntityType: "ProjectTask",
            cancellationToken: cancellationToken);
    }

    public async Task SendTaskReviewDecisionAsync(
        string userId,
        int taskId,
        string taskTitle,
        bool approved,
        string comment,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return;

        var eventType = approved ? EmailEventType.TaskReviewApproved : EmailEventType.TaskReviewRejected;
        var decision = approved ? "approved" : "rejected";
        var deduplicationKey = $"review-{(approved ? "approved" : "rejected")}:{taskId}:{userId}:{DateTime.UtcNow:yyyyMMddHHmm}";
        var subject = $"Task {(approved ? "approved" : "rejected")}: {taskTitle}";

        var html = $"""
            <h2>Task Review Decision</h2>
            <p>Hi {user.First_Name},</p>
            <p>Your task <strong>{taskTitle}</strong> (#{taskId}) has been <strong>{decision}</strong>.</p>
            <p>Reviewer comment: {comment}</p>
            <p>Log in to AgileFlow for more details.</p>
            <br/>
            <p>The AgileFlow Team</p>
            """;

        await SendAndLogAsync(
            recipientEmail: user.Email!,
            subject: subject,
            htmlBody: html,
            eventType: eventType,
            deduplicationKey: deduplicationKey,
            relatedEntityId: taskId,
            relatedEntityType: "ProjectTask",
            cancellationToken: cancellationToken);
    }

    public async Task SendDueDateReminderAsync(
        string userId,
        int taskId,
        string taskTitle,
        DateTime dueDate,
        string deduplicationKey,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return;

        // Deduplication check — skip if already sent for this window
        if (await _logRepository.ExistsByDeduplicationKeyAsync(deduplicationKey, cancellationToken))
        {
            _logger.LogDebug(
                "Due-date reminder skipped for task {TaskId}/user {UserId} — already sent (key: {Key})",
                taskId, userId, deduplicationKey);
            return;
        }

        var subject = $"Task due soon: {taskTitle}";
        var dueDateStr = dueDate.ToString("dddd, MMMM d 'at' h:mm tt UTC");

        var html = $"""
            <h2>Task Due Soon</h2>
            <p>Hi {user.First_Name},</p>
            <p>A friendly reminder that your task <strong>{taskTitle}</strong> (#{taskId})
               is due on <strong>{dueDateStr}</strong>.</p>
            <p>Log in to AgileFlow to check the current status.</p>
            <br/>
            <p>The AgileFlow Team</p>
            """;

        await SendAndLogAsync(
            recipientEmail: user.Email!,
            subject: subject,
            htmlBody: html,
            eventType: EmailEventType.DueDateReminder,
            deduplicationKey: deduplicationKey,
            relatedEntityId: taskId,
            relatedEntityType: "ProjectTask",
            cancellationToken: cancellationToken);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task SendAndLogAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        EmailEventType eventType,
        string deduplicationKey,
        int? relatedEntityId,
        string? relatedEntityType,
        CancellationToken cancellationToken)
    {
        EmailNotificationLog log;
        try
        {
            await _emailSender.SendAsync(recipientEmail, subject, htmlBody, cancellationToken: cancellationToken);
            log = EmailNotificationLog.CreateSuccess(
                recipientEmail, eventType, deduplicationKey, subject,
                relatedEntityId, relatedEntityType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send {EventType} email to {Email} (key: {Key})",
                eventType, recipientEmail, deduplicationKey);

            log = EmailNotificationLog.CreateFailure(
                recipientEmail, eventType, deduplicationKey, subject,
                ex.Message, relatedEntityId, relatedEntityType);
        }

        try
        {
            await _logRepository.AddAsync(log, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log persistence failure must never surface — we've already swallowed the send error
            _logger.LogError(ex,
                "Failed to persist EmailNotificationLog for {EventType} / {Email}", eventType, recipientEmail);
        }
    }
}
