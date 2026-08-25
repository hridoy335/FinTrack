using FinTrackCore.Application.Common.Errors;

namespace FinTrackCore.Application.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(ErrorCodes.NotFound, message, StatusCodes.NotFound)
    {
    }
}
