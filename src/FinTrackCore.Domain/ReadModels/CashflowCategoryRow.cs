namespace FinTrackCore.Domain.ReadModels;

public sealed class CashflowCategoryRow
{
    public required long CoaId { get; init; }
    public required string AccountCode { get; init; }
    public required string AccountName { get; init; }
    public required decimal Amount { get; init; }
}
