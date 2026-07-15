using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Interfaces;

/// <summary>
/// Persistence contract for <see cref="EmailNotificationLog"/> records.
/// </summary>
public interface IEmailNotificationLogRepository
{
    /// <summary>
    /// Returns <c>true</c> if a log entry with the given deduplication key already exists,
    /// regardless of its send status (even failed attempts block re-sends within the window).
    /// </summary>
    Task<bool> ExistsByDeduplicationKeyAsync(
        string deduplicationKey,
        CancellationToken cancellationToken = default);

    /// <summary>Persists a new log entry.</summary>
    Task AddAsync(
        EmailNotificationLog log,
        CancellationToken cancellationToken = default);
}
