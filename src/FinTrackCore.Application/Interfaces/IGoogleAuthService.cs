namespace FinTrackCore.Application.Interfaces;

public sealed class GoogleUserProfile
{
    public required string Subject { get; init; }
    public required string Email { get; init; }
    public string? GivenName { get; init; }
    public string? FamilyName { get; init; }
    public required bool EmailVerified { get; init; }
}

public interface IGoogleAuthService
{
    Task<GoogleUserProfile?> ValidateIdTokenAsync(
        string idToken,
        CancellationToken ct);
}
