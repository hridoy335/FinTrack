namespace FinTrackCore.Domain.ReadModels;

public sealed class AccountStatementLineRow
{
    public required long TransactionId { get; init; }
    public required DateTime TransactionDate { get; init; }
    public string? Description { get; init; }
    public required string TransactionTypeName { get; init; }
    public required decimal DebitAmount { get; init; }
    public required decimal CreditAmount { get; init; }
    public required string CounterpartyAccountName { get; init; }
}
