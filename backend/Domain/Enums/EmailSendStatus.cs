namespace Domain.Enums
{
    /// <summary>
    /// Outcome of an individual email send attempt.
    /// Used in <see cref="AgileFlow.Domain.Entities.EmailNotificationLog"/>.
    /// </summary>
    public enum EmailSendStatus
    {
        Sent,
        Failed,
    }
}
