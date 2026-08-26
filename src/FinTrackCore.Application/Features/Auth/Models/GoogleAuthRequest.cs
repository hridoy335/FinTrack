namespace FinTrackCore.Application.Features.Auth.Models;

public sealed class GoogleAuthRequest
{
    public required string IdToken { get; init; }
}
