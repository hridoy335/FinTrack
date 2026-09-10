namespace FinTrackCore.Domain.ReadModels;

public sealed class MonthlyCashflowRow
{
    public required int Year { get; init; }
    public required int Month { get; init; }
    public required decimal Income { get; init; }
    public required decimal Expense { get; init; }
}
