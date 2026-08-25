using FinTrackCore.Application.Common.Errors;

namespace FinTrackCore.Application.Common.Exceptions;

public sealed class ValidationException : AppException
{
    public ValidationException(string message, IReadOnlyList<string> errors)
        : base(ErrorCodes.ValidationFailed, message, StatusCodes.BadRequest, errors)
    {
    }

    public ValidationException(IReadOnlyList<string> errors)
        : this("One or more validation errors occurred.", errors)
    {
    }
}
