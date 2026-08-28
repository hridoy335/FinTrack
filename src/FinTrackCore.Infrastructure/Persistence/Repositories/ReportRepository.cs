using FinTrackCore.Domain;
using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.ReadModels;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinTrackCore.Infrastructure.Persistence.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly AppDbContext _dbContext;

    public ReportRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CoaBalanceRow>> GetCoaBalancesAsync(
        long userInfoId,
        long financialYearId,
        CancellationToken ct)
    {
        var voucherTotals = await (
            from line in _dbContext.VoucherLines.AsNoTracking()
            join transaction in _dbContext.Transactions.AsNoTracking()
                on line.TransactionId equals transaction.Id
            where transaction.UserInfoId == userInfoId
                  && transaction.FinancialYearId == financialYearId
            group line by line.CoaId
            into grouped
            select new
            {
                CoaId = grouped.Key,
                TotalDebit = grouped.Sum(x => x.DebitAmount),
                TotalCredit = grouped.Sum(x => x.CreditAmount)
            }).ToDictionaryAsync(x => x.CoaId, ct);

        var parentCoaIds = _dbContext.Coas
            .AsNoTracking()
            .Where(x => x.UserInfoId == userInfoId && x.ParentId != null)
            .Select(x => x.ParentId!.Value)
            .Distinct();

        var coas = await _dbContext.Coas
            .AsNoTracking()
            .Include(x => x.AccountType)
            .Where(x => x.UserInfoId == userInfoId
                        && x.IsActive
                        && (x.AccountTypeId == AccountTypeIds.Asset
                            || x.AccountTypeId == AccountTypeIds.Liability)
                        && !parentCoaIds.Contains(x.Id))
            .OrderBy(x => x.AccountCode)
            .ToListAsync(ct);

        return coas.Select(coa =>
        {
            voucherTotals.TryGetValue(coa.Id, out var totals);

            return new CoaBalanceRow
            {
                CoaId = coa.Id,
                AccountCode = coa.AccountCode,
                AccountName = coa.AccountName,
                AccountTypeId = coa.AccountTypeId,
                AccountTypeCode = coa.AccountType!.Code,
                NormalBalance = coa.AccountType.NormalBalance,
                TotalDebit = totals?.TotalDebit ?? 0,
                TotalCredit = totals?.TotalCredit ?? 0
            };
        }).ToList();
    }

    public Task<decimal> GetTransactionAmountTotalAsync(
        long userInfoId,
        long financialYearId,
        long transactionTypeId,
        DateTime fromDate,
        DateTime toDateExclusive,
        CancellationToken ct)
    {
        return _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserInfoId == userInfoId
                        && x.FinancialYearId == financialYearId
                        && x.TransactionTypeId == transactionTypeId
                        && x.TransactionDate >= fromDate
                        && x.TransactionDate < toDateExclusive)
            .SumAsync(x => x.Amount, ct);
    }
}
