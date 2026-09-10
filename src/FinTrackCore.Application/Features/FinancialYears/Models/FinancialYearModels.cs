namespace FinTrackCore.Application.Features.FinancialYears.Models;

public sealed class UpdateFinancialYearRequest
{
    public required string Name { get; init; }
    public bool IsClosed { get; init; }
}

public sealed class FinancialYearListItem
{
    public required long Id { get; init; }
    public required int Year { get; init; }
    public required string Name { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required bool IsActive { get; init; }
    public required bool IsClosed { get; init; }
    public required bool CanEdit { get; init; }
    public required bool IsCurrent { get; init; }
}
