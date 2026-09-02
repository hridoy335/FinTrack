namespace FinTrackCore.Domain.Entities;

public class UserInfo
{
    public long Id { get; private set; }

    public required string UserName { get; set; }
    public required string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? GoogleSubject { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required string CurrencyCode { get; set; }

    public required bool IsActive { get; set; }
    public required DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
