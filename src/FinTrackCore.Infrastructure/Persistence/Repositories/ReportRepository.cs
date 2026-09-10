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
        CancellationToken ct,
        DateTime? asOfDateExclusive = null,
        IReadOnlyList<long>? accountTypeIds = null)
    {
        var typeFilter = accountTypeIds ?? new long[] { AccountTypeIds.Asset, AccountTypeIds.Liability };

        var voucherTotals = await BuildVoucherTotalsQuery(
                userInfoId,
                financialYearId,
                asOfDateExclusive)
            .ToDictionaryAsync(x => x.CoaId, ct);

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
                        && typeFilter.Contains(x.AccountTypeId)
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

    public async Task<IReadOnlyList<CashflowCategoryRow>> GetCashflowCategoriesAsync(
        long userInfoId,
        long financialYearId,
        long transactionTypeId,
        DateTime fromDate,
        DateTime toDateExclusive,
        CancellationToken ct)
    {
        var isIncome = transactionTypeId == TransactionTypeIds.Income;

        var rows = await (
            from line in _dbContext.VoucherLines.AsNoTracking()
            join transaction in _dbContext.Transactions.AsNoTracking()
                on line.TransactionId equals transaction.Id
            join coa in _dbContext.Coas.AsNoTracking()
                on line.CoaId equals coa.Id
            where transaction.UserInfoId == userInfoId
                  && transaction.FinancialYearId == financialYearId
                  && transaction.TransactionTypeId == transactionTypeId
                  && transaction.TransactionDate >= fromDate
                  && transaction.TransactionDate < toDateExclusive
                  && (isIncome ? line.CreditAmount > 0 : line.DebitAmount > 0)
            group line by new { coa.Id, coa.AccountCode, coa.AccountName }
            into grouped
            orderby grouped.Key.AccountCode
            select new CashflowCategoryRow
            {
                CoaId = grouped.Key.Id,
                AccountCode = grouped.Key.AccountCode,
                AccountName = grouped.Key.AccountName,
                Amount = grouped.Sum(x => isIncome ? x.CreditAmount : x.DebitAmount)
            }).ToListAsync(ct);

        return rows;
    }

    public async Task<IReadOnlyList<MonthlyCashflowRow>> GetMonthlyCashflowAsync(
        long userInfoId,
        long financialYearId,
        DateTime fromDate,
        DateTime toDateExclusive,
        CancellationToken ct)
    {
        var transactions = await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserInfoId == userInfoId
                        && x.FinancialYearId == financialYearId
                        && (x.TransactionTypeId == TransactionTypeIds.Income
                            || x.TransactionTypeId == TransactionTypeIds.Expense)
                        && x.TransactionDate >= fromDate
                        && x.TransactionDate < toDateExclusive)
            .Select(x => new
            {
                x.TransactionDate.Year,
                x.TransactionDate.Month,
                x.TransactionTypeId,
                x.Amount
            })
            .ToListAsync(ct);

        return transactions
            .GroupBy(x => new { x.Year, x.Month })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(group => new MonthlyCashflowRow
            {
                Year = group.Key.Year,
                Month = group.Key.Month,
                Income = group
                    .Where(x => x.TransactionTypeId == TransactionTypeIds.Income)
                    .Sum(x => x.Amount),
                Expense = group
                    .Where(x => x.TransactionTypeId == TransactionTypeIds.Expense)
                    .Sum(x => x.Amount)
            })
            .ToList();
    }

    public async Task<CoaVoucherTotalsRow?> GetCoaVoucherTotalsAsync(
        long userInfoId,
        long financialYearId,
        long coaId,
        DateTime? fromDate,
        DateTime? toDateExclusive,
        CancellationToken ct)
    {
        var query =
            from line in _dbContext.VoucherLines.AsNoTracking()
            join transaction in _dbContext.Transactions.AsNoTracking()
                on line.TransactionId equals transaction.Id
            where transaction.UserInfoId == userInfoId
                  && transaction.FinancialYearId == financialYearId
                  && line.CoaId == coaId
            select new { line, transaction };

        if (fromDate is not null)
        {
            query = query.Where(x => x.transaction.TransactionDate >= fromDate.Value);
        }

        if (toDateExclusive is not null)
        {
            query = query.Where(x => x.transaction.TransactionDate < toDateExclusive.Value);
        }

        var totals = await query
            .GroupBy(_ => 1)
            .Select(group => new CoaVoucherTotalsRow
            {
                CoaId = coaId,
                TotalDebit = group.Sum(x => x.line.DebitAmount),
                TotalCredit = group.Sum(x => x.line.CreditAmount)
            })
            .FirstOrDefaultAsync(ct);

        return totals;
    }

    public async Task<IReadOnlyList<AccountStatementLineRow>> GetAccountStatementLinesAsync(
        long userInfoId,
        long financialYearId,
        long coaId,
        DateTime fromDate,
        DateTime toDateExclusive,
        CancellationToken ct)
    {
        var lines = await (
            from line in _dbContext.VoucherLines.AsNoTracking()
            join transaction in _dbContext.Transactions.AsNoTracking()
                on line.TransactionId equals transaction.Id
            join transactionType in _dbContext.TransactionTypes.AsNoTracking()
                on transaction.TransactionTypeId equals transactionType.Id
            where transaction.UserInfoId == userInfoId
                  && transaction.FinancialYearId == financialYearId
                  && line.CoaId == coaId
                  && transaction.TransactionDate >= fromDate
                  && transaction.TransactionDate < toDateExclusive
            orderby transaction.TransactionDate, transaction.Id
            select new
            {
                line.TransactionId,
                transaction.TransactionDate,
                transaction.Description,
                TransactionTypeName = transactionType.Name,
                line.DebitAmount,
                line.CreditAmount
            }).ToListAsync(ct);

        if (lines.Count == 0)
        {
            return Array.Empty<AccountStatementLineRow>();
        }

        var transactionIds = lines.Select(x => x.TransactionId).Distinct().ToList();

        var counterparties = await (
            from line in _dbContext.VoucherLines.AsNoTracking()
            join coa in _dbContext.Coas.AsNoTracking()
                on line.CoaId equals coa.Id
            where transactionIds.Contains(line.TransactionId) && line.CoaId != coaId
            select new
            {
                line.TransactionId,
                coa.AccountName
            }).ToListAsync(ct);

        var counterpartyLookup = counterparties
            .GroupBy(x => x.TransactionId)
            .ToDictionary(
                group => group.Key,
                group => string.Join(", ", group.Select(x => x.AccountName).Distinct()));

        return lines.Select(line => new AccountStatementLineRow
        {
            TransactionId = line.TransactionId,
            TransactionDate = line.TransactionDate,
            Description = line.Description,
            TransactionTypeName = line.TransactionTypeName,
            DebitAmount = line.DebitAmount,
            CreditAmount = line.CreditAmount,
            CounterpartyAccountName = counterpartyLookup.TryGetValue(line.TransactionId, out var name)
                ? name
                : "—"
        }).ToList();
    }

    private IQueryable<CoaVoucherTotalsRow> BuildVoucherTotalsQuery(
        long userInfoId,
        long financialYearId,
        DateTime? asOfDateExclusive)
    {
        var query =
            from line in _dbContext.VoucherLines.AsNoTracking()
            join transaction in _dbContext.Transactions.AsNoTracking()
                on line.TransactionId equals transaction.Id
            where transaction.UserInfoId == userInfoId
                  && transaction.FinancialYearId == financialYearId
            select new { line, transaction };

        if (asOfDateExclusive is not null)
        {
            query = query.Where(x => x.transaction.TransactionDate < asOfDateExclusive.Value);
        }

        return query
            .GroupBy(x => x.line.CoaId)
            .Select(group => new CoaVoucherTotalsRow
            {
                CoaId = group.Key,
                TotalDebit = group.Sum(x => x.line.DebitAmount),
                TotalCredit = group.Sum(x => x.line.CreditAmount)
            });
    }
}
