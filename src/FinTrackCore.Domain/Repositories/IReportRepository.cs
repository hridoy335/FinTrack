using FinTrackCore.Domain.ReadModels;

namespace FinTrackCore.Domain.Repositories;

public interface IReportRepository
{
    Task<IReadOnlyList<CoaBalanceRow>> GetCoaBalancesAsync(
        long userInfoId,
        long financialYearId,
        CancellationToken ct,
        DateTime? asOfDateExclusive = null,
        IReadOnlyList<long>? accountTypeIds = null);

    Task<decimal> GetTransactionAmountTotalAsync(
        long userInfoId,
        long financialYearId,
        long transactionTypeId,
        DateTime fromDate,
        DateTime toDateExclusive,
        CancellationToken ct);

    Task<IReadOnlyList<CashflowCategoryRow>> GetCashflowCategoriesAsync(
        long userInfoId,
        long financialYearId,
        long transactionTypeId,
        DateTime fromDate,
        DateTime toDateExclusive,
        CancellationToken ct);

    Task<IReadOnlyList<MonthlyCashflowRow>> GetMonthlyCashflowAsync(
        long userInfoId,
        long financialYearId,
        DateTime fromDate,
        DateTime toDateExclusive,
        CancellationToken ct);

    Task<CoaVoucherTotalsRow?> GetCoaVoucherTotalsAsync(
        long userInfoId,
        long financialYearId,
        long coaId,
        DateTime? fromDate,
        DateTime? toDateExclusive,
        CancellationToken ct);

    Task<IReadOnlyList<AccountStatementLineRow>> GetAccountStatementLinesAsync(
        long userInfoId,
        long financialYearId,
        long coaId,
        DateTime fromDate,
        DateTime toDateExclusive,
        CancellationToken ct);
}
