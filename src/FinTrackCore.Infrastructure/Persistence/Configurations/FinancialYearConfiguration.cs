using FinTrackCore.Application.Constants;
using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrackCore.Infrastructure.Persistence.Configurations;

public class FinancialYearConfiguration : IEntityTypeConfiguration<FinancialYear>
{
    public void Configure(EntityTypeBuilder<FinancialYear> builder)
    {
        builder.ToTable("FinancialYear");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.UserInfoId).IsRequired();
        builder.Property(x => x.Year).IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(LengthConstants.FinancialYearName)
            .IsRequired();

        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsClosed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedDate).IsRequired();
        builder.Property(x => x.UpdatedDate);

        builder.HasIndex(x => new { x.UserInfoId, x.Year }).IsUnique();
        builder.HasIndex(x => x.UserInfoId);

        builder.HasOne(x => x.UserInfo)
            .WithMany()
            .HasForeignKey(x => x.UserInfoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
