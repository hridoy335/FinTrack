using System.Text.RegularExpressions;
using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Constants;
using FinTrackCore.Application.Features.Auth.Models;
using FinTrackCore.Application.Features.Coas;
using FinTrackCore.Application.Features.FinancialYears;
using FinTrackCore.Application.Interfaces;
using FinTrackCore.Domain.Entities;
using FinTrackCore.Domain.Repositories;
using Microsoft.Extensions.Options;
using SharpOutcome;
using SharpOutcome.Helpers;
using SharpOutcome.Helpers.Enums;

namespace FinTrackCore.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IUserInfoRepository _userInfoRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IDefaultCoaSeedService _defaultCoaSeedService;
    private readonly IDefaultFinancialYearSeedService _defaultFinancialYearSeedService;
    private readonly MessageSettings _messages;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserInfoRepository userInfoRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        IGoogleAuthService googleAuthService,
        IDefaultCoaSeedService defaultCoaSeedService,
        IDefaultFinancialYearSeedService defaultFinancialYearSeedService,
        IOptions<MessageSettings> messageOptions,
        IOptions<JwtSettings> jwtOptions)
    {
        _userInfoRepository = userInfoRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _googleAuthService = googleAuthService;
        _defaultCoaSeedService = defaultCoaSeedService;
        _defaultFinancialYearSeedService = defaultFinancialYearSeedService;
        _messages = messageOptions.Value;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<Outcome<LoginResponse, HttpBadOutcome>> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        var loginKey = request.UserNameOrEmail.Trim();
        var user = await _userInfoRepository.GetByUserNameAsync(loginKey, ct);

        if (user is null)
        {
            user = await _userInfoRepository.GetByEmailAsync(loginKey.ToLowerInvariant(), ct);
        }

        if (user is null
            || !user.IsActive
            || !_passwordService.Verify(request.Password, user.PasswordHash))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Unauthorized, _messages.LoginFailed);
        }

        return await IssueTokensAsync(user, ipAddress, _messages.LoginSuccess, ct);
    }

    public async Task<Outcome<LoginResponse, HttpBadOutcome>> GoogleAsync(
        GoogleAuthRequest request,
        string? ipAddress,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        var profile = await _googleAuthService.ValidateIdTokenAsync(request.IdToken.Trim(), ct);
        if (profile is null || !profile.EmailVerified)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Unauthorized, _messages.GoogleAuthFailed);
        }

        var user = await _userInfoRepository.GetByGoogleSubjectAsync(profile.Subject, ct);

        if (user is null)
        {
            user = await _userInfoRepository.GetByEmailAsync(profile.Email, ct);

            if (user is not null)
            {
                if (!string.IsNullOrWhiteSpace(user.GoogleSubject)
                    && !string.Equals(user.GoogleSubject, profile.Subject, StringComparison.Ordinal))
                {
                    return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.Conflict);
                }

                user.GoogleSubject = profile.Subject;
                user.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Update(user);
                await _unitOfWork.SaveChangesAsync(ct);
            }
            else
            {
                user = new UserInfo
                {
                    UserName = await CreateUniqueUserNameAsync(profile.Email, ct),
                    Email = profile.Email,
                    PasswordHash = null,
                    GoogleSubject = profile.Subject,
                    FirstName = string.IsNullOrWhiteSpace(profile.GivenName)
                        ? profile.Email.Split('@')[0]
                        : profile.GivenName.Trim(),
                    LastName = string.IsNullOrWhiteSpace(profile.FamilyName)
                        ? null
                        : profile.FamilyName.Trim(),
                    CurrencyCode = CurrencyConstants.DefaultCurrencyCode,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    await _unitOfWork.AddAsync(user, innerCt);
                    await _unitOfWork.SaveChangesAsync(innerCt);
                    await _defaultCoaSeedService.SeedForUserAsync(user.Id, innerCt);
                    await _defaultFinancialYearSeedService.SeedForUserAsync(user.Id, innerCt);
                }, ct);
            }
        }

        if (!user.IsActive)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Unauthorized, _messages.GoogleAuthFailed);
        }

        return await IssueTokensAsync(user, ipAddress, _messages.GoogleAuthSuccess, ct);
    }

    public async Task<Outcome<LoginResponse, HttpBadOutcome>> RefreshAsync(
        RefreshRequest request,
        string? ipAddress,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken.Trim());
        var existing = await _refreshTokenRepository.GetActiveByTokenHashAsync(tokenHash, ct);

        if (existing is null || existing.UserInfo is null || !existing.UserInfo.IsActive)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Unauthorized, _messages.InvalidRefreshToken);
        }

        var user = existing.UserInfo;
        var (plainRefreshToken, refreshTokenHash, refreshExpiresAt) = _jwtTokenService.CreateRefreshToken();

        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            existing.RevokedAt = DateTime.UtcNow;
            existing.IsActive = false;
            existing.ReplacedByTokenHash = refreshTokenHash;
            _unitOfWork.Update(existing);

            var replacement = new RefreshToken
            {
                UserInfoId = user.Id,
                TokenHash = refreshTokenHash,
                ExpiresAt = refreshExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                IsActive = true
            };

            await _unitOfWork.AddAsync(replacement, innerCt);
            await _unitOfWork.SaveChangesAsync(innerCt);
        }, ct);

        return new LoginResponse
        {
            AccessToken = _jwtTokenService.CreateAccessToken(user),
            RefreshToken = plainRefreshToken,
            ExpiresIn = _jwtSettings.AccessTokenMinutes * AuthConstants.SecondsPerMinute,
            Message = _messages.TokenRefreshed,
            User = MapUser(user)
        };
    }

    public async Task<Outcome<LogoutResponse, HttpBadOutcome>> LogoutAsync(
        LogoutRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken.Trim());
        var existing = await _refreshTokenRepository.GetActiveByTokenHashAsync(tokenHash, ct);

        if (existing is not null)
        {
            existing.RevokedAt = DateTime.UtcNow;
            existing.IsActive = false;
            _unitOfWork.Update(existing);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return new LogoutResponse { Message = _messages.LogoutSuccess };
    }

    private async Task<LoginResponse> IssueTokensAsync(
        UserInfo user,
        string? ipAddress,
        string message,
        CancellationToken ct)
    {
        var accessToken = _jwtTokenService.CreateAccessToken(user);
        var (plainRefreshToken, refreshTokenHash, refreshExpiresAt) = _jwtTokenService.CreateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserInfoId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            IsActive = true
        };

        await _unitOfWork.AddAsync(refreshToken, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = plainRefreshToken,
            ExpiresIn = _jwtSettings.AccessTokenMinutes * AuthConstants.SecondsPerMinute,
            Message = message,
            User = MapUser(user)
        };
    }

    private async Task<string> CreateUniqueUserNameAsync(string email, CancellationToken ct)
    {
        var localPart = email.Split('@')[0];
        var baseName = Regex.Replace(localPart, @"[^a-zA-Z0-9._-]", string.Empty);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = AuthConstants.DefaultGeneratedUserName;
        }

        if (baseName.Length > AuthConstants.MaxGeneratedUserNameLength)
        {
            baseName = baseName[..AuthConstants.MaxGeneratedUserNameLength];
        }

        var candidate = baseName;
        var suffix = 0;
        while (await _userInfoRepository.GetByUserNameAsync(candidate, ct) is not null)
        {
            suffix++;
            candidate = $"{baseName}{suffix}";
        }

        return candidate;
    }

    private static LoginUserDto MapUser(UserInfo user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        CurrencyCode = user.CurrencyCode
    };
}
