using Domain.Enums;

namespace AgileFlow.Domain.Entities
{
    /// <summary>
    /// Audit record for every email send attempt.
    /// The DeduplicationKey unique index prevents duplicate sends
    /// (e.g. a second due-date reminder for the same task/user/date window).
    /// </summary>
    public class EmailNotificationLog
    {
        public int Id { get; private set; }

        /// <summary>Email address the message was sent (or attempted) to.</summary>
        public string RecipientEmail { get; private set; } = string.Empty;

        public EmailEventType EventType { get; private set; }

        /// <summary>PK of the related entity (task id, workspace id, etc.).</summary>
        public int? RelatedEntityId { get; private set; }

        /// <summary>Human-readable entity type name, e.g. "ProjectTask", "Workspace".</summary>
        public string? RelatedEntityType { get; private set; }

        /// <summary>
        /// Globally-unique key used to prevent duplicate sends.
        /// Pattern examples:
        ///   due:{taskId}:{userId}:{dueDate:yyyy-MM-dd}
        ///   assign:{taskId}:{userId}:{timestamp:yyyyMMddHHmm}
        ///   workspace-invite:{workspaceId}:{userId}:{timestamp:yyyyMMddHHmm}
        /// </summary>
        public string DeduplicationKey { get; private set; } = string.Empty;

        public string Subject { get; private set; } = string.Empty;

        public EmailSendStatus Status { get; private set; }

        /// <summary>Populated when Status == Failed.</summary>
        public string? ErrorMessage { get; private set; }

        public DateTime? SentAt { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private EmailNotificationLog() { }

        public static EmailNotificationLog CreateSuccess(
            string recipientEmail,
            EmailEventType eventType,
            string deduplicationKey,
            string subject,
            int? relatedEntityId = null,
            string? relatedEntityType = null)
        {
            return new EmailNotificationLog
            {
                RecipientEmail = recipientEmail,
                EventType = eventType,
                DeduplicationKey = deduplicationKey,
                Subject = subject,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                Status = EmailSendStatus.Sent,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
            };
        }

        public static EmailNotificationLog CreateFailure(
            string recipientEmail,
            EmailEventType eventType,
            string deduplicationKey,
            string subject,
            string errorMessage,
            int? relatedEntityId = null,
            string? relatedEntityType = null)
        {
            return new EmailNotificationLog
            {
                RecipientEmail = recipientEmail,
                EventType = eventType,
                DeduplicationKey = deduplicationKey,
                Subject = subject,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                Status = EmailSendStatus.Failed,
                ErrorMessage = errorMessage,
                CreatedAt = DateTime.UtcNow,
            };
        }
    }
}
