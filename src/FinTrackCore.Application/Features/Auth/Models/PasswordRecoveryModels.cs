namespace FinTrackCore.Application.Features.Auth.Models;

public sealed class ForgotPasswordRequest
{
    public required string Email { get; set; }
}

public sealed class VerifyRecoveryCodeRequest
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}

public sealed class ResetPasswordRequest
{
    public required string Email { get; set; }
    public required string Code { get; set; }
    public required string NewPassword { get; set; }
}

public sealed class PasswordRecoveryMessageResponse
{
    public required string Message { get; init; }
    public required int ExpiresInMinutes { get; init; }
}
