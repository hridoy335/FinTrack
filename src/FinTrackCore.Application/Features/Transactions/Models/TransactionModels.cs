namespace FinTrackCore.Application.Features.Transactions.Models;

public sealed class CreateTransactionRequest
{
    public required long TransactionTypeId { get; init; }
    public required long FinancialYearId { get; init; }
    public required DateTime TransactionDate { get; init; }
    public required decimal Amount { get; init; }
    public string? Description { get; init; }
    public required long DebitCoaId { get; init; }
    public required long CreditCoaId { get; init; }
}

public sealed class TransactionListQuery
{
    public long? FinancialYearId { get; init; }
    public long? TransactionTypeId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; }
}
