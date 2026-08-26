using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetActiveByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var query =
            from refreshToken in dbContext.RefreshTokens
            where refreshToken.TokenHash == tokenHash
                  && refreshToken.RevokedAt == null
                  && refreshToken.ExpiresAt > now
            select refreshToken;

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
