namespace FinTrackCore.Application.Common.Models;

public sealed class ErrorResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;
    public IReadOnlyList<string>? Errors { get; init; }
    public string? TraceId { get; init; }
}
