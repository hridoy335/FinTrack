using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface ICoaRepository
{
    Task<Coa> GetByIdForUserAsync(long id, long userInfoId, CancellationToken ct);
    Task<bool> ExistsByCodeForUserAsync(string accountCode, long userInfoId, CancellationToken ct);
    Task<IReadOnlyList<Coa>> GetAllForUserAsync(long userInfoId, CancellationToken ct);
    Task<bool> HasChildrenAsync(long id, long userInfoId, CancellationToken ct);
}
