namespace FinTrackCore.Domain.Entities;

public class RefreshToken
{
    public long Id { get; private set; }

    public required long UserInfoId { get; set; }
    public UserInfo? UserInfo { get; set; }

    public required string TokenHash { get; set; }

    public required DateTime ExpiresAt { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }

    public required bool IsActive { get; set; }
}
