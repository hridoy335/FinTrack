using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrackCore.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.UserInfoId).IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.RevokedAt);

        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(500);
        builder.Property(x => x.CreatedByIp).HasMaxLength(100);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserInfoId);

        builder.HasOne(x => x.UserInfo)
            .WithMany()
            .HasForeignKey(x => x.UserInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.IsActive);
    }
}
