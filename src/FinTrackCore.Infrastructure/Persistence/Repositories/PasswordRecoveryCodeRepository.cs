using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class PasswordRecoveryCodeRepository : IPasswordRecoveryCodeRepository
{
    private readonly AppDbContext _dbContext;

    public PasswordRecoveryCodeRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsActiveCodeHashAsync(string codeHash, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        return _dbContext.PasswordRecoveryCodes.AnyAsync(
            x => x.CodeHash == codeHash
                 && x.IsActive
                 && x.UsedAt == null
                 && x.ExpiresAt > now,
            ct);
    }

    public Task<PasswordRecoveryCode?> GetActiveByEmailAndCodeHashAsync(
        string email,
        string codeHash,
        DateTime utcNow,
        CancellationToken ct)
    {
        return _dbContext.PasswordRecoveryCodes
            .Include(x => x.UserInfo)
            .FirstOrDefaultAsync(
                x => x.CodeHash == codeHash
                     && x.IsActive
                     && x.UsedAt == null
                     && x.ExpiresAt > utcNow
                     && x.UserInfo != null
                     && x.UserInfo.Email == email,
                ct);
    }

    public async Task<IReadOnlyList<PasswordRecoveryCode>> GetActiveByUserIdAsync(
        long userInfoId,
        DateTime utcNow,
        CancellationToken ct)
    {
        return await _dbContext.PasswordRecoveryCodes
            .Where(x => x.UserInfoId == userInfoId
                        && x.IsActive
                        && x.UsedAt == null
                        && x.ExpiresAt > utcNow)
            .ToListAsync(ct);
    }
}
