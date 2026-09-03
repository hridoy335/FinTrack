namespace FinTrackCore.Application.Common.Models;

public sealed class MutationResult
{
    public required long Id { get; init; }
    public required string Message { get; init; }
    public string? AccountCode { get; init; }
}
