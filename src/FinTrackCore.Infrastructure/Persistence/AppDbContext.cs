using FinTrackCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserInfo> UserInfos => Set<UserInfo>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AccountType> AccountTypes => Set<AccountType>();
    public DbSet<Coa> Coas => Set<Coa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
