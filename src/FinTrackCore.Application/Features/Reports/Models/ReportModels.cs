namespace FinTrackCore.Application.Features.Reports.Models;

public sealed class CashflowReportQuery
{
    public long? FinancialYearId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class BalanceReportQuery
{
    public long? FinancialYearId { get; init; }
    public DateTime? AsOfDate { get; init; }
}

public sealed class AccountStatementQuery
{
    public required long CoaId { get; init; }
    public long? FinancialYearId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class MonthlyCashflowReportQuery
{
    public long? FinancialYearId { get; init; }
}

public sealed class CashflowReportResponse
{
    public required long FinancialYearId { get; init; }
    public required int FinancialYear { get; init; }
    public required DateTime FromDate { get; init; }
    public required DateTime ToDate { get; init; }
    public required decimal TotalInflow { get; init; }
    public required decimal TotalOutflow { get; init; }
    public required decimal NetCashflow { get; init; }
    public required IReadOnlyList<CashflowCategoryItem> Inflows { get; init; }
    public required IReadOnlyList<CashflowCategoryItem> Outflows { get; init; }
}

public sealed class CashflowCategoryItem
{
    public required long CoaId { get; init; }
    public required string AccountCode { get; init; }
    public required string AccountName { get; init; }
    public required decimal Amount { get; init; }
}

public sealed class BalanceReportResponse
{
    public required long FinancialYearId { get; init; }
    public required int FinancialYear { get; init; }
    public required DateTime AsOfDate { get; init; }
    public required decimal TotalAssets { get; init; }
    public required decimal TotalLiabilities { get; init; }
    public required decimal TotalEquity { get; init; }
    public required decimal NetWorth { get; init; }
    public required IReadOnlyList<BalanceSectionItem> Sections { get; init; }
}

public sealed class BalanceSectionItem
{
    public required long AccountTypeId { get; init; }
    public required string AccountTypeCode { get; init; }
    public required string AccountTypeName { get; init; }
    public required decimal Subtotal { get; init; }
    public required IReadOnlyList<AccountBalanceItem> Accounts { get; init; }
}

public sealed class AccountStatementResponse
{
    public required long CoaId { get; init; }
    public required string AccountCode { get; init; }
    public required string AccountName { get; init; }
    public required string AccountTypeCode { get; init; }
    public required long FinancialYearId { get; init; }
    public required int FinancialYear { get; init; }
    public required DateTime FromDate { get; init; }
    public required DateTime ToDate { get; init; }
    public required decimal OpeningBalance { get; init; }
    public required decimal ClosingBalance { get; init; }
    public required IReadOnlyList<AccountStatementLineItem> Lines { get; init; }
}

public sealed class AccountStatementLineItem
{
    public required long TransactionId { get; init; }
    public required DateTime TransactionDate { get; init; }
    public string? Description { get; init; }
    public required string TransactionTypeName { get; init; }
    public required decimal Debit { get; init; }
    public required decimal Credit { get; init; }
    public required decimal Balance { get; init; }
    public required string CounterpartyAccountName { get; init; }
}

public sealed class MonthlyCashflowReportResponse
{
    public required long FinancialYearId { get; init; }
    public required int FinancialYear { get; init; }
    public required IReadOnlyList<MonthlyCashflowItem> Months { get; init; }
    public required decimal TotalIncome { get; init; }
    public required decimal TotalExpense { get; init; }
    public required decimal TotalNet { get; init; }
}

public sealed class MonthlyCashflowItem
{
    public required int Year { get; init; }
    public required int Month { get; init; }
    public required string Label { get; init; }
    public required decimal Income { get; init; }
    public required decimal Expense { get; init; }
    public required decimal Net { get; init; }
}
