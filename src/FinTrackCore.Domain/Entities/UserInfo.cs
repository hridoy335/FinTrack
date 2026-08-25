namespace FinTrackCore.Domain.Entities;

public class UserInfo
{
    public long Id { get; private set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required string CurrencyCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; } 
    public DateTime? UpdatedDate { get; set; }
}
