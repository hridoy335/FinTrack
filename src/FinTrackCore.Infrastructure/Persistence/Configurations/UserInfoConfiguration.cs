using FinTrackCore.Application.Constants;
using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrackCore.Infrastructure.Persistence.Configurations;

public class UserInfoConfiguration : IEntityTypeConfiguration<UserInfo>
{
    public void Configure(EntityTypeBuilder<UserInfo> builder)
    {
        builder.ToTable("UserInfo");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Email)
            .HasMaxLength(LengthConstants.Email)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(LengthConstants.PasswordHash);

        builder.Property(x => x.GoogleSubject)
            .HasMaxLength(LengthConstants.GoogleSubject);

        builder.Property(x => x.FirstName)
            .HasMaxLength(LengthConstants.PersonName)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(LengthConstants.PersonName);

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(LengthConstants.CurrencyCode)
            .IsRequired()
            .HasDefaultValue(CurrencyConstants.DefaultCurrencyCode);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.UpdatedDate);

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.GoogleSubject)
            .IsUnique()
            .HasFilter("\"GoogleSubject\" IS NOT NULL");
    }
}
