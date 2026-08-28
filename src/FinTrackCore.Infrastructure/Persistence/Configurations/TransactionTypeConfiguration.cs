using FinTrackCore.Application.Constants;
using FinTrackCore.Domain;
using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrackCore.Infrastructure.Persistence.Configurations;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder.ToTable("TransactionType");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasMaxLength(LengthConstants.TransactionTypeCode)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(LengthConstants.TransactionTypeName)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasData(
            new TransactionType
            {
                Id = TransactionTypeIds.Income,
                Code = TransactionTypeConstants.Codes.Income,
                Name = TransactionTypeConstants.Names.Income
            },
            new TransactionType
            {
                Id = TransactionTypeIds.Expense,
                Code = TransactionTypeConstants.Codes.Expense,
                Name = TransactionTypeConstants.Names.Expense
            },
            new TransactionType
            {
                Id = TransactionTypeIds.Transfer,
                Code = TransactionTypeConstants.Codes.Transfer,
                Name = TransactionTypeConstants.Names.Transfer
            },
            new TransactionType
            {
                Id = TransactionTypeIds.OpeningBalance,
                Code = TransactionTypeConstants.Codes.OpeningBalance,
                Name = TransactionTypeConstants.Names.OpeningBalance
            });
    }
}
