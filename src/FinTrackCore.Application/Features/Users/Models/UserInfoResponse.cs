namespace FinTrackCore.Application.Features.Users.Models;

public sealed class UserInfoResponse
{
    public long Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string? LastName { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}
