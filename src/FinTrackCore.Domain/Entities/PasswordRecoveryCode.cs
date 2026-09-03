namespace FinTrackCore.Domain.Entities;

public class PasswordRecoveryCode
{
    public long Id { get; private set; }

    public required long UserInfoId { get; set; }
    public UserInfo? UserInfo { get; set; }

    public required string CodeHash { get; set; }

    public required DateTime ExpiresAt { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public required bool IsActive { get; set; }
}
