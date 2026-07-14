using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFlow.Infrastructure.Persistence.Configurations
{
    public class TaskDependentConfiguration : IEntityTypeConfiguration<TaskDependent>
    {
        public void Configure(EntityTypeBuilder<TaskDependent> builder)
        {
            builder.HasKey(td => new { td.TaskId, td.DependedTaskId });

            builder.HasQueryFilter(td =>
                !td.Task.IsDeleted &&
                !td.DependedTask.IsDeleted);

            builder.HasOne(td => td.Task)
                .WithMany(t => t.TaskDependents)
                .HasForeignKey(td => td.TaskId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(td => td.DependedTask)
                .WithMany()
                .HasForeignKey(td => td.DependedTaskId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

