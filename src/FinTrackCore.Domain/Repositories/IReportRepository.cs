using FinTrackCore.Domain.ReadModels;

namespace FinTrackCore.Domain.Repositories;

public interface IReportRepository
{
    Task<IReadOnlyList<CoaBalanceRow>> GetCoaBalancesAsync(
        long userInfoId,
        long financialYearId,
        CancellationToken ct);

    Task<decimal> GetTransactionAmountTotalAsync(
        long userInfoId,
        long financialYearId,
        long transactionTypeId,
        DateTime fromDate,
        DateTime toDateExclusive,
        CancellationToken ct);
}
