using FinTrackCore.Application.Constants;
using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrackCore.Infrastructure.Persistence.Configurations;

public class PasswordRecoveryCodeConfiguration : IEntityTypeConfiguration<PasswordRecoveryCode>
{
    public void Configure(EntityTypeBuilder<PasswordRecoveryCode> builder)
    {
        builder.ToTable("PasswordRecoveryCode");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.UserInfoId).IsRequired();

        builder.Property(x => x.CodeHash)
            .HasMaxLength(LengthConstants.RecoveryCodeHash)
            .IsRequired();

        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UsedAt);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(x => x.CodeHash).IsUnique();
        builder.HasIndex(x => x.UserInfoId);

        builder.HasOne(x => x.UserInfo)
            .WithMany()
            .HasForeignKey(x => x.UserInfoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
