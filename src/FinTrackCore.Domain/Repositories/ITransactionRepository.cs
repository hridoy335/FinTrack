using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> GetByIdForUserAsync(long id, long userInfoId, CancellationToken ct);

    Task<(IReadOnlyList<Transaction> Items, long TotalCount)> GetPagedForUserAsync(
        long userInfoId,
        long? financialYearId,
        long? transactionTypeId,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken ct);
}
