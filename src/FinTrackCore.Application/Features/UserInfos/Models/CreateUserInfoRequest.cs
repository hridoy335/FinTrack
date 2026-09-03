namespace FinTrackCore.Application.Features.UserInfos.Models;

public sealed class CreateUserInfoRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CurrencyCode { get; set; }
}
