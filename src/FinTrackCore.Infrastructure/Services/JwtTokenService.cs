using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Interfaces;
using FinTrackCore.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinTrackCore.Infrastructure.Services;

public class JwtTokenService(IOptions<JwtSettings> jwtOptions) : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public string CreateAccessToken(UserInfo user)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.Key) || _jwtSettings.Key.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be configured and at least 32 characters.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new("uid", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string PlainToken, string TokenHash, DateTime ExpiresAt) CreateRefreshToken()
    {
        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = HashRefreshToken(plainToken);
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);
        return (plainToken, tokenHash, expiresAt);
    }

    public string HashRefreshToken(string plainToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToHexString(bytes);
    }
}
