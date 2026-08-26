using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
