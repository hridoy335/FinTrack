using FinTrackCore.Application.Constants;
using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrackCore.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transaction");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.UserInfoId).IsRequired();
        builder.Property(x => x.FinancialYearId).IsRequired();
        builder.Property(x => x.TransactionTypeId).IsRequired();
        builder.Property(x => x.TransactionDate).IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(TransactionConstants.AmountPrecision, TransactionConstants.AmountScale)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(LengthConstants.TransactionDescription);

        builder.Property(x => x.CreatedDate).IsRequired();
        builder.Property(x => x.UpdatedDate);

        builder.HasIndex(x => x.UserInfoId);
        builder.HasIndex(x => x.FinancialYearId);
        builder.HasIndex(x => x.TransactionTypeId);
        builder.HasIndex(x => new { x.UserInfoId, x.TransactionDate });

        builder.HasOne(x => x.UserInfo)
            .WithMany()
            .HasForeignKey(x => x.UserInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FinancialYear)
            .WithMany()
            .HasForeignKey(x => x.FinancialYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TransactionType)
            .WithMany()
            .HasForeignKey(x => x.TransactionTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
