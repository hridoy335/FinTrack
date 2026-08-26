namespace FinTrackCore.Application.Common.Models;

public sealed class ErrorResponse
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
    public required string ErrorCode { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
    public string? TraceId { get; init; }
}
