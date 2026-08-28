using FinTrackCore.Application.Constants;
using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrackCore.Infrastructure.Persistence.Configurations;

public class VoucherLineConfiguration : IEntityTypeConfiguration<VoucherLine>
{
    public void Configure(EntityTypeBuilder<VoucherLine> builder)
    {
        builder.ToTable("VoucherLine");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.TransactionId).IsRequired();
        builder.Property(x => x.CoaId).IsRequired();
        builder.Property(x => x.LineNumber).IsRequired();

        builder.Property(x => x.DebitAmount)
            .HasPrecision(TransactionConstants.AmountPrecision, TransactionConstants.AmountScale)
            .IsRequired();

        builder.Property(x => x.CreditAmount)
            .HasPrecision(TransactionConstants.AmountPrecision, TransactionConstants.AmountScale)
            .IsRequired();

        builder.Property(x => x.CreatedDate).IsRequired();

        builder.HasIndex(x => x.TransactionId);
        builder.HasIndex(x => x.CoaId);
        builder.HasIndex(x => new { x.TransactionId, x.LineNumber }).IsUnique();

        builder.HasOne(x => x.Transaction)
            .WithMany(x => x.VoucherLines)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Coa)
            .WithMany()
            .HasForeignKey(x => x.CoaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
