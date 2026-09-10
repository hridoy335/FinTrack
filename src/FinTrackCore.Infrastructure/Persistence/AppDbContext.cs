using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserInfo> UserInfos => Set<UserInfo>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordRecoveryCode> PasswordRecoveryCodes => Set<PasswordRecoveryCode>();
    public DbSet<AccountType> AccountTypes => Set<AccountType>();
    public DbSet<Coa> Coas => Set<Coa>();
    public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();
    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<VoucherLine> VoucherLines => Set<VoucherLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
