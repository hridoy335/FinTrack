using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetActiveByTokenHashAsync(
        string tokenHash,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        return await dbContext.RefreshTokens
            .Include(x => x.UserInfo)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash
                     && x.IsActive
                     && x.RevokedAt == null
                     && x.ExpiresAt > now,
                ct);
    }
}
