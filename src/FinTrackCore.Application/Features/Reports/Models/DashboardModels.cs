namespace FinTrackCore.Application.Features.Reports.Models;

public sealed class DashboardReportQuery
{
    public long? FinancialYearId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class DashboardResponse
{
    public required long FinancialYearId { get; init; }
    public required int FinancialYear { get; init; }
    public required decimal TotalBalance { get; init; }
    public required decimal IncomeThisMonth { get; init; }
    public required decimal ExpenseThisMonth { get; init; }
    public required decimal NetThisMonth { get; init; }
    public required IReadOnlyList<AccountBalanceItem> AccountBalances { get; init; }
    public required IReadOnlyList<CashflowCategoryItem> ExpenseCategoriesThisMonth { get; init; }
}

public sealed class AccountBalanceItem
{
    public required long CoaId { get; init; }
    public required string AccountCode { get; init; }
    public required string AccountName { get; init; }
    public required long AccountTypeId { get; init; }
    public required string AccountTypeCode { get; init; }
    public required decimal Balance { get; init; }
}
