namespace FinTrackCore.Application.Features.Auth.Models;

public sealed class LogoutRequest
{
    public required string RefreshToken { get; init; }
}
