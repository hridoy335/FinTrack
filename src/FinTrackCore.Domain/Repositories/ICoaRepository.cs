using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface ICoaRepository
{
    Task<Coa> GetByIdForUserAsync(long id, long userInfoId, CancellationToken ct);
    Task<bool> ExistsByCodeForUserAsync(string accountCode, long userInfoId, CancellationToken ct);
    Task<bool> ExistsByAccountNameForUserAndAccountTypeAsync(
        long userInfoId,
        long accountTypeId,
        string accountName,
        long? excludeCoaId,
        CancellationToken ct);
    Task<IReadOnlyList<string>> GetAccountCodesForUserAndAccountTypeAsync(
        long userInfoId,
        long accountTypeId,
        CancellationToken ct);
    Task<IReadOnlyList<Coa>> GetAllForUserAsync(long userInfoId, CancellationToken ct);
    Task<bool> HasChildrenAsync(long id, long userInfoId, CancellationToken ct);
    Task<IReadOnlySet<long>> GetCoaIdsUsedInTransactionsAsync(long userInfoId, CancellationToken ct);
    Task<bool> IsUsedInTransactionsAsync(long coaId, long userInfoId, CancellationToken ct);
}
