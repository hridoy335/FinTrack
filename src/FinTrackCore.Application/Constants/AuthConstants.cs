namespace FinTrackCore.Application.Constants;

public static class AuthConstants
{
    public const int AccessTokenMinutesDefault = 15;
    public const int RefreshTokenDaysDefault = 7;
    public const int JwtMinKeyLength = 32;
    public const int RefreshTokenByteLength = 64;
    public const int SecondsPerMinute = 60;
    public const int MaxGeneratedUserNameLength = 80;
    public const string DefaultGeneratedUserName = "user";
}
