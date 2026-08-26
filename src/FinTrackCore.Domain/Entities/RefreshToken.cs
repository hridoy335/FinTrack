namespace FinTrackCore.Domain.Entities;

public class RefreshToken
{
    public long Id { get; private set; }

    public long UserInfoId { get; set; }
    public UserInfo UserInfo { get; set; } = null!;

    public required string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;
}
