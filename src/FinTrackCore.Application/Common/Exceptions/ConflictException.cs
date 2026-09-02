using FinTrackCore.Application.Common.Errors;

namespace FinTrackCore.Application.Common.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base(ErrorCodes.Conflict, message, StatusCodes.Conflict)
    {
    }
}
