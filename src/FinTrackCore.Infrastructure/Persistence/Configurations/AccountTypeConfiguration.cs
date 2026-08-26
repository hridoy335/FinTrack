using FinTrackCore.Application.Constants;
using FinTrackCore.Domain;
using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrackCore.Infrastructure.Persistence.Configurations;

public class AccountTypeConfiguration : IEntityTypeConfiguration<AccountType>
{
    public void Configure(EntityTypeBuilder<AccountType> builder)
    {
        builder.ToTable("AccountType");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasMaxLength(LengthConstants.AccountTypeCode)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(LengthConstants.AccountTypeName)
            .IsRequired();

        builder.Property(x => x.NormalBalance)
            .HasMaxLength(LengthConstants.NormalBalance)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasData(
            new AccountType
            {
                Id = AccountTypeIds.Asset,
                Code = AccountTypeConstants.Codes.Asset,
                Name = AccountTypeConstants.Names.Asset,
                NormalBalance = Domain.NormalBalance.Debit
            },
            new AccountType
            {
                Id = AccountTypeIds.Liability,
                Code = AccountTypeConstants.Codes.Liability,
                Name = AccountTypeConstants.Names.Liability,
                NormalBalance = Domain.NormalBalance.Credit
            },
            new AccountType
            {
                Id = AccountTypeIds.Equity,
                Code = AccountTypeConstants.Codes.Equity,
                Name = AccountTypeConstants.Names.Equity,
                NormalBalance = Domain.NormalBalance.Credit
            },
            new AccountType
            {
                Id = AccountTypeIds.Income,
                Code = AccountTypeConstants.Codes.Income,
                Name = AccountTypeConstants.Names.Income,
                NormalBalance = Domain.NormalBalance.Credit
            },
            new AccountType
            {
                Id = AccountTypeIds.Expense,
                Code = AccountTypeConstants.Codes.Expense,
                Name = AccountTypeConstants.Names.Expense,
                NormalBalance = Domain.NormalBalance.Debit
            });
    }
}
