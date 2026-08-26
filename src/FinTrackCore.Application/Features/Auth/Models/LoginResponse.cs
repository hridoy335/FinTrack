namespace FinTrackCore.Application.Features.Auth.Models;

public sealed class LoginUserDto
{
    public long Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string? LastName { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
}

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
    public string Message { get; init; } = string.Empty;
    public LoginUserDto User { get; init; } = null!;
}
