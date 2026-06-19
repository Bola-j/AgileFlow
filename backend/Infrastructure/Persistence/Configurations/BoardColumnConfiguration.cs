using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileFlow.Infrastructure.Persistence.Configurations
{
    public class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
    {
        public void Configure(EntityTypeBuilder<BoardColumn> builder)
        {
            builder.HasKey(bc => bc.Id);

            builder.Property(bc => bc.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(bc => bc.CreatedAt)
                .IsRequired();

            builder.Property(bc => bc.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(bc => !bc.IsDeleted);

            builder.HasOne(bc => bc.Board)
                .WithMany(b => b.BoardColumns)
                .HasForeignKey(bc => bc.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(bc => bc.Tasks)
                .WithOne(t => t.Column)
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

