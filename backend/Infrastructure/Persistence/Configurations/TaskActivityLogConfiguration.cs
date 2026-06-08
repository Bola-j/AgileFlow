using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public class TaskActivityLogConfiguration : IEntityTypeConfiguration<TaskActivityLog>
    {
        public void Configure(EntityTypeBuilder<TaskActivityLog> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.FieldChanged)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(l => l.NewValue)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(l => l.OldValue)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(l => l.CreatedAt)
                .IsRequired();

            builder.HasOne(l => l.ProjectTask)
                .WithMany(t => t.TaskActivityLogs)
                .HasForeignKey(l => l.ProjectTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(l => l.AppUser)
                .WithMany(u => u.TaskActivityLogs)
                .HasForeignKey(l => l.AppUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
