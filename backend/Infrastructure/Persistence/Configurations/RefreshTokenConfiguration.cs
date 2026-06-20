using AgileFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgileFlow.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(r => r.Token)
            .IsUnique();   // fast lookup on refresh, prevents duplicates

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.ExpiresAt).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.IsRevoked).IsRequired().HasDefaultValue(false);

        builder.HasOne(r => r.AppUser)
            .WithMany()                     // AppUser has no nav back to RefreshTokens
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}