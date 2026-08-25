namespace FinTrackCore.Application.Features.Users.Models;

public sealed class UpdateUserInfoRequest
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public string? Password { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public string CurrencyCode { get; set; } = "BDT";
    public bool IsActive { get; set; } = true;
}
