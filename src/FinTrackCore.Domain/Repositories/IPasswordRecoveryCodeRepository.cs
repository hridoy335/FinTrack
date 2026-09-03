using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface IPasswordRecoveryCodeRepository
{
    Task<bool> ExistsActiveCodeHashAsync(string codeHash, CancellationToken ct);

    Task<PasswordRecoveryCode?> GetActiveByEmailAndCodeHashAsync(
        string email,
        string codeHash,
        DateTime utcNow,
        CancellationToken ct);

    Task<IReadOnlyList<PasswordRecoveryCode>> GetActiveByUserIdAsync(
        long userInfoId,
        DateTime utcNow,
        CancellationToken ct);
}
