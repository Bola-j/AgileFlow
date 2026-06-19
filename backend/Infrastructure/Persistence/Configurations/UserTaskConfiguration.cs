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
    public class UserTaskConfiguration : IEntityTypeConfiguration<UserTask>
    {
        public void Configure(EntityTypeBuilder<UserTask> builder)
        {
            builder.HasKey(ut => new { ut.AppUserId, ut.ProjectTaskId });

            builder.Property(ut => ut.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(ut => !ut.IsDeleted);

            builder.HasOne(ut => ut.AppUser)
                .WithMany(u => u.UserTasks)
                .HasForeignKey(ut => ut.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ut => ut.ProjectTask)
                .WithMany(t => t.UserTasks)
                .HasForeignKey(ut => ut.ProjectTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

