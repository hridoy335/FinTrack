namespace FinTrackCore.Application.Features.Auth.Models;

public sealed class LoginUserDto
{
    public required long Id { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public string? LastName { get; init; }
    public required string CurrencyCode { get; init; }
}

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required string Message { get; init; }
    public required LoginUserDto User { get; init; }
}
