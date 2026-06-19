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
    public class UserWorkspaceConfiguration : IEntityTypeConfiguration<UserWorkspace>
    {
        public void Configure(EntityTypeBuilder<UserWorkspace> builder)
        {
            builder.HasKey(uw => new { uw.AppUserId, uw.WorkspaceId });

            builder.Property(uw => uw.JoinedAt)
                .IsRequired();

            builder.Property(uw => uw.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(uw => !uw.IsDeleted);

            builder.HasOne(uw => uw.AppUser)
                .WithMany(u => u.UserWorkspaces)
                .HasForeignKey(uw => uw.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uw => uw.Workspace)
                .WithMany(w => w.UserWorkspaces)
                .HasForeignKey(uw => uw.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

