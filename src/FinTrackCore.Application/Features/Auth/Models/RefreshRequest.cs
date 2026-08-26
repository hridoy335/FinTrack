namespace FinTrackCore.Application.Features.Auth.Models;

public sealed class RefreshRequest
{
    public required string RefreshToken { get; init; }
}
