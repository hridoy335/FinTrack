using FinTrackCore.Application.Constants;
using FinTrackCore.Application.Features.Reports.Models;
using FinTrackCore.Domain;
using FinTrackCore.Domain.ReadModels;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Application.Features.FinancialYears;
using SharpOutcome;
using SharpOutcome.Helpers;
using SharpOutcome.Helpers.Enums;

namespace FinTrackCore.Application.Features.Reports;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IFinancialYearRepository _financialYearRepository;
    private readonly IFinancialYearService _financialYearService;
    private readonly ICoaRepository _coaRepository;

    public ReportService(
        IReportRepository reportRepository,
        IFinancialYearRepository financialYearRepository,
        IFinancialYearService financialYearService,
        ICoaRepository coaRepository)
    {
        _reportRepository = reportRepository;
        _financialYearRepository = financialYearRepository;
        _financialYearService = financialYearService;
        _coaRepository = coaRepository;
    }

    public Task<Outcome<DashboardResponse, HttpBadOutcome>> GetDashboardAsync(
        long userInfoId,
        DashboardReportQuery query,
        CancellationToken ct) =>
        BuildDashboardAsync(userInfoId, query, ct);

    public async Task<Outcome<CashflowReportResponse, HttpBadOutcome>> GetCashflowAsync(
        long userInfoId,
        CashflowReportQuery query,
        CancellationToken ct)
    {
        var yearResult = await ResolveFinancialYearAsync(userInfoId, query.FinancialYearId, ct);
        if (yearResult.TryPickBadOutcome(out var yearError))
        {
            return yearError;
        }

        yearResult.TryPickGoodOutcome(out var financialYear);
        var (fromDate, toDateExclusive) = ResolveDateRange(financialYear!, query.FromDate, query.ToDate);

        var inflows = await _reportRepository.GetCashflowCategoriesAsync(
            userInfoId,
            financialYear!.Id,
            TransactionTypeIds.Income,
            fromDate,
            toDateExclusive,
            ct);

        var outflows = await _reportRepository.GetCashflowCategoriesAsync(
            userInfoId,
            financialYear.Id,
            TransactionTypeIds.Expense,
            fromDate,
            toDateExclusive,
            ct);

        var totalInflow = inflows.Sum(x => x.Amount);
        var totalOutflow = outflows.Sum(x => x.Amount);

        return new CashflowReportResponse
        {
            FinancialYearId = financialYear.Id,
            FinancialYear = financialYear.Year,
            FromDate = fromDate,
            ToDate = toDateExclusive.AddDays(-1),
            TotalInflow = Round(totalInflow),
            TotalOutflow = Round(totalOutflow),
            NetCashflow = Round(totalInflow - totalOutflow),
            Inflows = MapCashflowCategories(inflows),
            Outflows = MapCashflowCategories(outflows)
        };
    }

    public async Task<Outcome<BalanceReportResponse, HttpBadOutcome>> GetBalanceAsync(
        long userInfoId,
        BalanceReportQuery query,
        CancellationToken ct)
    {
        var yearResult = await ResolveFinancialYearAsync(userInfoId, query.FinancialYearId, ct);
        if (yearResult.TryPickBadOutcome(out var yearError))
        {
            return yearError;
        }

        yearResult.TryPickGoodOutcome(out var financialYear);
        var asOfDate = query.AsOfDate?.Date ?? DateTime.UtcNow.Date;
        if (asOfDate > financialYear!.EndDate.Date)
        {
            asOfDate = financialYear.EndDate.Date;
        }

        if (asOfDate < financialYear.StartDate.Date)
        {
            asOfDate = financialYear.StartDate.Date;
        }

        var asOfDateExclusive = ToUtcDate(asOfDate).AddDays(1);
        var accountTypeIds = new long[]
        {
            AccountTypeIds.Asset,
            AccountTypeIds.Liability,
            AccountTypeIds.Equity
        };

        var coaBalances = await _reportRepository.GetCoaBalancesAsync(
            userInfoId,
            financialYear.Id,
            ct,
            asOfDateExclusive,
            accountTypeIds);

        var accounts = coaBalances.Select(MapAccountBalance).ToList();
        var sections = accounts
            .GroupBy(x => new { x.AccountTypeId, x.AccountTypeCode })
            .OrderBy(x => x.Key.AccountTypeId)
            .Select(group =>
            {
                var sectionAccounts = group.OrderBy(x => x.AccountCode).ToList();
                return new BalanceSectionItem
                {
                    AccountTypeId = group.Key.AccountTypeId,
                    AccountTypeCode = group.Key.AccountTypeCode,
                    AccountTypeName = FormatAccountTypeName(group.Key.AccountTypeCode),
                    Subtotal = Round(sectionAccounts.Sum(x => x.Balance)),
                    Accounts = sectionAccounts
                };
            })
            .ToList();

        var totalAssets = Round(accounts.Where(x => x.AccountTypeId == AccountTypeIds.Asset).Sum(x => x.Balance));
        var totalLiabilities = Round(accounts.Where(x => x.AccountTypeId == AccountTypeIds.Liability).Sum(x => x.Balance));
        var totalEquity = Round(accounts.Where(x => x.AccountTypeId == AccountTypeIds.Equity).Sum(x => x.Balance));

        return new BalanceReportResponse
        {
            FinancialYearId = financialYear.Id,
            FinancialYear = financialYear.Year,
            AsOfDate = asOfDate,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            NetWorth = Round(totalAssets - totalLiabilities),
            Sections = sections
        };
    }

    public async Task<Outcome<AccountStatementResponse, HttpBadOutcome>> GetAccountStatementAsync(
        long userInfoId,
        AccountStatementQuery query,
        CancellationToken ct)
    {
        if (query.CoaId <= 0)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, "Account is required.");
        }

        var coa = await _coaRepository.GetByIdForUserAsync(query.CoaId, userInfoId, ct);

        var yearResult = await ResolveFinancialYearAsync(userInfoId, query.FinancialYearId, ct);
        if (yearResult.TryPickBadOutcome(out var yearError))
        {
            return yearError;
        }

        yearResult.TryPickGoodOutcome(out var financialYear);
        var (fromDate, toDateExclusive) = ResolveDateRange(financialYear!, query.FromDate, query.ToDate);

        var openingTotals = await _reportRepository.GetCoaVoucherTotalsAsync(
            userInfoId,
            financialYear!.Id,
            coa.Id,
            null,
            fromDate,
            ct);

        var openingBalance = ComputeSignedBalance(
            coa.AccountType!.NormalBalance,
            openingTotals?.TotalDebit ?? 0,
            openingTotals?.TotalCredit ?? 0);

        var rawLines = await _reportRepository.GetAccountStatementLinesAsync(
            userInfoId,
            financialYear.Id,
            coa.Id,
            fromDate,
            toDateExclusive,
            ct);

        var runningBalance = openingBalance;
        var lines = rawLines.Select(row =>
        {
            var movement = ComputeSignedBalance(
                coa.AccountType.NormalBalance,
                row.DebitAmount,
                row.CreditAmount);
            runningBalance += movement;

            return new AccountStatementLineItem
            {
                TransactionId = row.TransactionId,
                TransactionDate = row.TransactionDate,
                Description = row.Description,
                TransactionTypeName = row.TransactionTypeName,
                Debit = Round(row.DebitAmount),
                Credit = Round(row.CreditAmount),
                Balance = Round(runningBalance),
                CounterpartyAccountName = row.CounterpartyAccountName
            };
        }).ToList();

        return new AccountStatementResponse
        {
            CoaId = coa.Id,
            AccountCode = coa.AccountCode,
            AccountName = coa.AccountName,
            AccountTypeCode = coa.AccountType!.Code,
            FinancialYearId = financialYear.Id,
            FinancialYear = financialYear.Year,
            FromDate = fromDate,
            ToDate = toDateExclusive.AddDays(-1),
            OpeningBalance = Round(openingBalance),
            ClosingBalance = Round(runningBalance),
            Lines = lines
        };
    }

    public async Task<Outcome<MonthlyCashflowReportResponse, HttpBadOutcome>> GetMonthlyCashflowAsync(
        long userInfoId,
        MonthlyCashflowReportQuery query,
        CancellationToken ct)
    {
        var yearResult = await ResolveFinancialYearAsync(userInfoId, query.FinancialYearId, ct);
        if (yearResult.TryPickBadOutcome(out var yearError))
        {
            return yearError;
        }

        yearResult.TryPickGoodOutcome(out var financialYear);
        var fromDate = ToUtcDate(financialYear!.StartDate.Date);
        var toDateExclusive = ToUtcDate(financialYear.EndDate.Date).AddDays(1);

        var rows = await _reportRepository.GetMonthlyCashflowAsync(
            userInfoId,
            financialYear.Id,
            fromDate,
            toDateExclusive,
            ct);

        var months = rows
            .Select(row => new MonthlyCashflowItem
            {
                Year = row.Year,
                Month = row.Month,
                Label = new DateTime(row.Year, row.Month, 1).ToString("MMM yyyy"),
                Income = Round(row.Income),
                Expense = Round(row.Expense),
                Net = Round(row.Income - row.Expense)
            })
            .ToList();

        var totalIncome = Round(months.Sum(x => x.Income));
        var totalExpense = Round(months.Sum(x => x.Expense));

        return new MonthlyCashflowReportResponse
        {
            FinancialYearId = financialYear.Id,
            FinancialYear = financialYear.Year,
            Months = months,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            TotalNet = Round(totalIncome - totalExpense)
        };
    }

    private async Task<Outcome<DashboardResponse, HttpBadOutcome>> BuildDashboardAsync(
        long userInfoId,
        DashboardReportQuery query,
        CancellationToken ct)
    {
        var yearResult = await ResolveFinancialYearAsync(userInfoId, query.FinancialYearId, ct);
        if (yearResult.TryPickBadOutcome(out var error))
        {
            return error;
        }

        yearResult.TryPickGoodOutcome(out var financialYear);

        var coaBalances = await _reportRepository.GetCoaBalancesAsync(userInfoId, financialYear!.Id, ct);
        var accountBalances = coaBalances.Select(MapAccountBalance).ToList();

        var (monthStart, monthEndExclusive) = ResolveDashboardMonthRange(
            financialYear!,
            query.FromDate,
            query.ToDate);

        var incomeThisMonth = await _reportRepository.GetTransactionAmountTotalAsync(
            userInfoId,
            financialYear.Id,
            TransactionTypeIds.Income,
            monthStart,
            monthEndExclusive,
            ct);

        var expenseThisMonth = await _reportRepository.GetTransactionAmountTotalAsync(
            userInfoId,
            financialYear.Id,
            TransactionTypeIds.Expense,
            monthStart,
            monthEndExclusive,
            ct);

        var expenseCategoriesThisMonth = MapCashflowCategories(
            await _reportRepository.GetCashflowCategoriesAsync(
                userInfoId,
                financialYear.Id,
                TransactionTypeIds.Expense,
                monthStart,
                monthEndExclusive,
                ct));

        var assetTotal = accountBalances
            .Where(x => x.AccountTypeId == AccountTypeIds.Asset)
            .Sum(x => x.Balance);

        var liabilityTotal = accountBalances
            .Where(x => x.AccountTypeId == AccountTypeIds.Liability)
            .Sum(x => x.Balance);

        return new DashboardResponse
        {
            FinancialYearId = financialYear.Id,
            FinancialYear = financialYear.Year,
            TotalBalance = Round(assetTotal - liabilityTotal),
            IncomeThisMonth = Round(incomeThisMonth),
            ExpenseThisMonth = Round(expenseThisMonth),
            NetThisMonth = Round(incomeThisMonth - expenseThisMonth),
            AccountBalances = accountBalances,
            ExpenseCategoriesThisMonth = expenseCategoriesThisMonth
        };
    }

    private async Task<Outcome<Domain.Entities.FinancialYear, HttpBadOutcome>> ResolveFinancialYearAsync(
        long userInfoId,
        long? financialYearId,
        CancellationToken ct)
    {
        if (financialYearId is null)
        {
            return await _financialYearService.GetCurrentAsync(userInfoId, ct);
        }

        var financialYear = await _financialYearRepository.GetByIdForUserAsync(
            financialYearId.Value,
            userInfoId,
            ct);

        return financialYear;
    }

    private static (DateTime FromDate, DateTime ToDateExclusive) ResolveDateRange(
        Domain.Entities.FinancialYear financialYear,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var fyStart = ToUtcDate(financialYear.StartDate.Date);
        var fyEndExclusive = ToUtcDate(financialYear.EndDate.Date).AddDays(1);

        var resolvedFrom = fromDate.HasValue ? ToUtcDate(fromDate.Value.Date) : fyStart;
        var resolvedToExclusive = toDate.HasValue
            ? ToUtcDate(toDate.Value.Date).AddDays(1)
            : fyEndExclusive;

        if (resolvedFrom < fyStart)
        {
            resolvedFrom = fyStart;
        }

        if (resolvedToExclusive > fyEndExclusive)
        {
            resolvedToExclusive = fyEndExclusive;
        }

        if (resolvedFrom >= resolvedToExclusive)
        {
            resolvedToExclusive = resolvedFrom.AddDays(1);
        }

        return (resolvedFrom, resolvedToExclusive);
    }

    private static (DateTime FromDate, DateTime ToDateExclusive) ResolveDashboardMonthRange(
        Domain.Entities.FinancialYear financialYear,
        DateTime? fromDate,
        DateTime? toDate)
    {
        if (fromDate.HasValue && toDate.HasValue)
        {
            var fyStart = ToUtcDate(financialYear.StartDate.Date);
            var fyEndExclusive = ToUtcDate(financialYear.EndDate.Date).AddDays(1);

            var resolvedFrom = ToUtcDate(fromDate.Value.Date);
            var resolvedToExclusive = ToUtcDate(toDate.Value.Date).AddDays(1);

            if (resolvedFrom < fyStart)
            {
                resolvedFrom = fyStart;
            }

            if (resolvedToExclusive > fyEndExclusive)
            {
                resolvedToExclusive = fyEndExclusive;
            }

            if (resolvedFrom < resolvedToExclusive)
            {
                return (resolvedFrom, resolvedToExclusive);
            }
        }

        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return (monthStart, monthStart.AddMonths(1));
    }

    private static IReadOnlyList<CashflowCategoryItem> MapCashflowCategories(
        IReadOnlyList<CashflowCategoryRow> rows) =>
        rows
            .Select(row => new CashflowCategoryItem
            {
                CoaId = row.CoaId,
                AccountCode = row.AccountCode,
                AccountName = row.AccountName,
                Amount = Round(row.Amount)
            })
            .ToList();

    private static AccountBalanceItem MapAccountBalance(CoaBalanceRow row)
    {
        var balance = ComputeSignedBalance(row.NormalBalance, row.TotalDebit, row.TotalCredit);

        return new AccountBalanceItem
        {
            CoaId = row.CoaId,
            AccountCode = row.AccountCode,
            AccountName = row.AccountName,
            AccountTypeId = row.AccountTypeId,
            AccountTypeCode = row.AccountTypeCode,
            Balance = Round(balance)
        };
    }

    private static decimal ComputeSignedBalance(
        string normalBalance,
        decimal totalDebit,
        decimal totalCredit) =>
        normalBalance == NormalBalance.Debit
            ? totalDebit - totalCredit
            : totalCredit - totalDebit;

    private static string FormatAccountTypeName(string accountTypeCode) =>
        accountTypeCode switch
        {
            "ASSET" => "Assets",
            "LIABILITY" => "Liabilities",
            "EQUITY" => "Equity",
            "INCOME" => "Income",
            "EXPENSE" => "Expenses",
            _ => accountTypeCode
        };

    private static decimal Round(decimal value) =>
        decimal.Round(value, TransactionConstants.AmountScale);

    private static DateTime ToUtcDate(DateTime value) =>
        DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
}
