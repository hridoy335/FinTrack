using FinTrackCore.Application.Constants;
using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrackCore.Infrastructure.Persistence.Configurations;

public class CoaConfiguration : IEntityTypeConfiguration<Coa>
{
    public void Configure(EntityTypeBuilder<Coa> builder)
    {
        builder.ToTable("Coa");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.UserInfoId).IsRequired();
        builder.Property(x => x.ParentId);
        builder.Property(x => x.AccountTypeId).IsRequired();

        builder.Property(x => x.AccountCode)
            .HasMaxLength(LengthConstants.AccountCode)
            .IsRequired();

        builder.Property(x => x.AccountName)
            .HasMaxLength(LengthConstants.AccountName)
            .IsRequired();

        builder.Property(x => x.IsSystemDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedDate).IsRequired();
        builder.Property(x => x.UpdatedDate);

        builder.HasIndex(x => new { x.UserInfoId, x.AccountCode }).IsUnique();
        builder.HasIndex(x => x.UserInfoId);
        builder.HasIndex(x => x.AccountTypeId);
        builder.HasIndex(x => x.ParentId);

        builder.HasOne(x => x.UserInfo)
            .WithMany()
            .HasForeignKey(x => x.UserInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AccountType)
            .WithMany()
            .HasForeignKey(x => x.AccountTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
