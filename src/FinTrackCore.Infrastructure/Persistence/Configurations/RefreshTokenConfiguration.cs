using FinTrackCore.Application.Constants;
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
            .HasMaxLength(LengthConstants.TokenHash)
            .IsRequired();

        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.RevokedAt);

        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(LengthConstants.TokenHash);
        builder.Property(x => x.CreatedByIp).HasMaxLength(LengthConstants.IpAddress);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserInfoId);

        builder.HasOne(x => x.UserInfo)
            .WithMany()
            .HasForeignKey(x => x.UserInfoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
