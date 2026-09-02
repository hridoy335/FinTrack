namespace FinTrackCore.Application.Features.Auth.Models;

public sealed class LoginRequest
{
    public required string UserNameOrEmail { get; set; }
    public required string Password { get; set; }
}
