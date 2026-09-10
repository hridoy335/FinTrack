namespace FinTrackCore.Domain.ReadModels;

public sealed class CoaVoucherTotalsRow
{
    public required long CoaId { get; init; }
    public required decimal TotalDebit { get; init; }
    public required decimal TotalCredit { get; init; }
}
