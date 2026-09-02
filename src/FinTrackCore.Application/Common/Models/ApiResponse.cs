namespace FinTrackCore.Application.Common.Models;

public class ApiResponse<T>
{
    public required bool Success { get; init; }
    public required int StatusCode { get; init; }
    public required string Message { get; init; }
    public T? Data { get; init; }
    public ApiMeta? Meta { get; init; }
}

public class ApiMeta
{
    public required long TotalData { get; init; }
    public required long TotalPage { get; init; }
}
