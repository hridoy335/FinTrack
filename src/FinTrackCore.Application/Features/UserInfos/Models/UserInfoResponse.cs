namespace FinTrackCore.Application.Features.UserInfos.Models;

public sealed class UserInfoResponse
{
    public required long Id { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public string? LastName { get; init; }
    public required string CurrencyCode { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
}
