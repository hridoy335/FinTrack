using FinTrackCore.Domain.Entities;

namespace FinTrackCore.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateAccessToken(UserInfo user);
    (string PlainToken, string TokenHash, DateTime ExpiresAt) CreateRefreshToken();
    string HashRefreshToken(string plainToken);
}
