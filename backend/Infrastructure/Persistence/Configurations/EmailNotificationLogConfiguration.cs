using AgileFlow.Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgileFlow.Infrastructure.Persistence.Configurations;

public class EmailNotificationLogConfiguration : IEntityTypeConfiguration<EmailNotificationLog>
{
    public void Configure(EntityTypeBuilder<EmailNotificationLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.RecipientEmail)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(l => l.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.RelatedEntityType)
            .HasMaxLength(100);

        builder.Property(l => l.DeduplicationKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        // Unique index on DeduplicationKey prevents duplicate sends within a dedup window
        builder.HasIndex(l => l.DeduplicationKey)
            .IsUnique()
            .HasDatabaseName("IX_EmailNotificationLogs_DeduplicationKey");

        // Supporting indexes for queries / reporting
        builder.HasIndex(l => new { l.RecipientEmail, l.EventType })
            .HasDatabaseName("IX_EmailNotificationLogs_Recipient_EventType");

        builder.HasIndex(l => l.CreatedAt)
            .HasDatabaseName("IX_EmailNotificationLogs_CreatedAt");
    }
}
