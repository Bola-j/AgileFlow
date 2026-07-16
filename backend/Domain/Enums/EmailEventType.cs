namespace Domain.Enums
{
    /// <summary>
    /// Identifies the business event that triggered an email notification.
    /// Used in <see cref="AgileFlow.Domain.Entities.EmailNotificationLog"/>.
    /// </summary>
    public enum EmailEventType
    {
        EmailVerification,
        WorkspaceInvite,
        TaskAssigned,
        TaskSubmittedForReview,
        TaskReviewApproved,
        TaskReviewRejected,
        DueDateReminder,
    }
}
