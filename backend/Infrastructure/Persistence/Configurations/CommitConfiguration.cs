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
    public class CommitConfiguration : IEntityTypeConfiguration<Commit>
    {
        public void Configure(EntityTypeBuilder<Commit> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.BranchName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.CommitHash)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.URL)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.Status)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(c => !c.IsDeleted);

            builder.HasOne(c => c.ProjectTask)
                .WithMany(t => t.Commits)
                .HasForeignKey(c => c.ProjectTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.AppUser)
                .WithMany(u => u.Commits)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
