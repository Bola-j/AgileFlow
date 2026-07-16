using AgileFlow.Application.Interfaces;
using AgileFlow.Domain.Entities;
using AgileFlow.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace AgileFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for <see cref="EmailNotificationLog"/>.
/// </summary>
public sealed class EmailNotificationLogRepository : IEmailNotificationLogRepository
{
    private readonly AgileFlowDbContext _context;

    public EmailNotificationLogRepository(AgileFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByDeduplicationKeyAsync(
        string deduplicationKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.EmailNotificationLogs
            .AnyAsync(l => l.DeduplicationKey == deduplicationKey, cancellationToken);
    }

    public async Task AddAsync(
        EmailNotificationLog log,
        CancellationToken cancellationToken = default)
    {
        await _context.EmailNotificationLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
