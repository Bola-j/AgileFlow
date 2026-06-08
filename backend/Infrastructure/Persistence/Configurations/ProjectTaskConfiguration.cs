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
    public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
    {
        public void Configure(EntityTypeBuilder<ProjectTask> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(t => t.Status)
                .IsRequired();

            builder.Property(t => t.Priority)
                .IsRequired();

            builder.Property(t => t.DueDate)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(t => !t.IsDeleted);

            builder.HasOne(t => t.Column)
                .WithMany(bc => bc.Tasks)
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(t => t.Sprint)
                .WithMany(s => s.Tasks)
                .HasForeignKey(t => t.SprintId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(t => t.UserTasks)
                .WithOne(ut => ut.ProjectTask)
                .HasForeignKey(ut => ut.ProjectTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Comments)
                .WithOne(c => c.ProjectTask)
                .HasForeignKey(c => c.ProjectTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Commits)
                .WithOne(c => c.ProjectTask)
                .HasForeignKey(c => c.ProjectTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.TaskActivityLogs)
                .WithOne(l => l.ProjectTask)
                .HasForeignKey(l => l.ProjectTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.TaskDependents)
                .WithOne(td => td.Task)
                .HasForeignKey(td => td.TaskId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
