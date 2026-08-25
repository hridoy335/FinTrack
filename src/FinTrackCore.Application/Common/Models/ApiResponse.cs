namespace FinTrackCore.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public ApiMeta? Meta { get; init; }
}

public class ApiMeta
{
    public long TotalData { get; init; }
    public long TotalPage { get; init; }
}
