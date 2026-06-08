using AgileFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(u => u.First_Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Last_Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Profile_Picture)
                .HasMaxLength(500);

            builder.Property(u => u.Github_Username)
                .HasMaxLength(100);

            builder.Property(u => u.DOB)
                .HasColumnType("date");

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(u => !u.IsDeleted);

        }
    }
}
