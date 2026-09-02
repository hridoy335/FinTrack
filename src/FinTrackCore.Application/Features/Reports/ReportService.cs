using FinTrackCore.Application.Constants;
using FinTrackCore.Application.Features.Reports.Models;
using FinTrackCore.Domain;
using FinTrackCore.Domain.ReadModels;
using FinTrackCore.Domain.Repositories;
using FinTrackCore.Application.Features.FinancialYears;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.Reports;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IFinancialYearRepository _financialYearRepository;
    private readonly IFinancialYearService _financialYearService;

    public ReportService(
        IReportRepository reportRepository,
        IFinancialYearRepository financialYearRepository,
        IFinancialYearService financialYearService)
    {
        _reportRepository = reportRepository;
        _financialYearRepository = financialYearRepository;
        _financialYearService = financialYearService;
    }

    public async Task<Outcome<DashboardResponse, HttpBadOutcome>> GetDashboardAsync(
        long userInfoId,
        long? financialYearId,
        CancellationToken ct)
    {
        Domain.Entities.FinancialYear financialYear;

        if (financialYearId is null)
        {
            var currentYearResult = await _financialYearService.GetCurrentAsync(userInfoId, ct);

            if (currentYearResult.TryPickBadOutcome(out var error))
            {
                return error;
            }

            currentYearResult.TryPickGoodOutcome(out var resolvedYear);
            financialYear = resolvedYear!;
        }
        else
        {
            financialYear = await _financialYearRepository.GetByIdForUserAsync(
                financialYearId.Value,
                userInfoId,
                ct);
        }

        var coaBalances = await _reportRepository.GetCoaBalancesAsync(userInfoId, financialYear.Id, ct);

        var accountBalances = coaBalances
            .Select(MapAccountBalance)
            .ToList();

        var monthStart = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);
        var monthEndExclusive = monthStart.AddMonths(1);

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
            TotalBalance = decimal.Round(assetTotal - liabilityTotal, TransactionConstants.AmountScale),
            IncomeThisMonth = decimal.Round(incomeThisMonth, TransactionConstants.AmountScale),
            ExpenseThisMonth = decimal.Round(expenseThisMonth, TransactionConstants.AmountScale),
            NetThisMonth = decimal.Round(incomeThisMonth - expenseThisMonth, TransactionConstants.AmountScale),
            AccountBalances = accountBalances
        };
    }

    private static AccountBalanceItem MapAccountBalance(CoaBalanceRow row)
    {
        var balance = row.NormalBalance == NormalBalance.Debit
            ? row.TotalDebit - row.TotalCredit
            : row.TotalCredit - row.TotalDebit;

        return new AccountBalanceItem
        {
            CoaId = row.CoaId,
            AccountCode = row.AccountCode,
            AccountName = row.AccountName,
            AccountTypeId = row.AccountTypeId,
            AccountTypeCode = row.AccountTypeCode,
            Balance = decimal.Round(balance, TransactionConstants.AmountScale)
        };
    }
}
