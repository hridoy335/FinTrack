namespace FinTrackCore.Application.Common.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "FinTrackCore";
    public string Audience { get; set; } = "FinTrackCore";
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
