namespace FinTrackCore.Application.Common.Exceptions;

public class AppException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public IReadOnlyList<string>? Errors { get; }

    public AppException(
        string errorCode,
        string message,
        int statusCode = StatusCodes.BadRequest,
        IReadOnlyList<string>? errors = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Errors = errors;
    }

    public static class StatusCodes
    {
        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int Conflict = 409;
        public const int InternalServerError = 500;
    }
}
