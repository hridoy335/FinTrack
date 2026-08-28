namespace FinTrackCore.Domain.ReadModels;

public sealed class CoaBalanceRow
{
    public required long CoaId { get; init; }
    public required string AccountCode { get; init; }
    public required string AccountName { get; init; }
    public required long AccountTypeId { get; init; }
    public required string AccountTypeCode { get; init; }
    public required string NormalBalance { get; init; }
    public required decimal TotalDebit { get; init; }
    public required decimal TotalCredit { get; init; }
}
