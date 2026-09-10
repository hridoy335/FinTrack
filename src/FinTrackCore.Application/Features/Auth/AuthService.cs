using System.Globalization;
using System.Security.Cryptography;
using System.Text;
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
    private readonly IPasswordRecoveryCodeRepository _passwordRecoveryCodeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IEmailSender _emailSender;
    private readonly IDefaultCoaSeedService _defaultCoaSeedService;
    private readonly IDefaultFinancialYearSeedService _defaultFinancialYearSeedService;
    private readonly MessageSettings _messages;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserInfoRepository userInfoRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordRecoveryCodeRepository passwordRecoveryCodeRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        IGoogleAuthService googleAuthService,
        IEmailSender emailSender,
        IDefaultCoaSeedService defaultCoaSeedService,
        IDefaultFinancialYearSeedService defaultFinancialYearSeedService,
        IOptions<MessageSettings> messageOptions,
        IOptions<JwtSettings> jwtOptions)
    {
        _userInfoRepository = userInfoRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordRecoveryCodeRepository = passwordRecoveryCodeRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _googleAuthService = googleAuthService;
        _emailSender = emailSender;
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
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userInfoRepository.GetByEmailAsync(email, ct);

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
                    return new HttpBadOutcome(HttpBadOutcomeTag.Conflict, _messages.GoogleEmailLinkedToOtherAccount);
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

    public Task<Outcome<PasswordRecoveryMessageResponse, HttpBadOutcome>> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken ct)
    {
        return SendRecoveryCodeAsync(request, ct);
    }

    public async Task<Outcome<PasswordRecoveryMessageResponse, HttpBadOutcome>> VerifyRecoveryCodeAsync(
        VerifyRecoveryCodeRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        var recovery = await FindActiveRecoveryAsync(request.Email, request.Code, ct);
        if (recovery is null)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.RecoveryCodeInvalid);
        }

        return new PasswordRecoveryMessageResponse
        {
            Message = _messages.RecoveryCodeVerified,
            ExpiresInMinutes = PasswordRecoveryConstants.ExpiryMinutes
        };
    }

    public async Task<Outcome<PasswordRecoveryMessageResponse, HttpBadOutcome>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Code)
            || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return new HttpBadOutcome(
                HttpBadOutcomeTag.BadRequest,
                string.IsNullOrWhiteSpace(request.NewPassword)
                    ? _messages.InvalidNewPassword
                    : _messages.ValidationFailed);
        }

        var recovery = await FindActiveRecoveryAsync(request.Email, request.Code, ct);
        if (recovery?.UserInfo is null)
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.RecoveryCodeInvalid);
        }

        var user = recovery.UserInfo;
        var now = DateTime.UtcNow;

        user.PasswordHash = _passwordService.Hash(request.NewPassword);
        user.UpdatedDate = now;
        _unitOfWork.Update(user);

        recovery.UsedAt = now;
        recovery.IsActive = false;
        _unitOfWork.Update(recovery);

        var otherActiveCodes = await _passwordRecoveryCodeRepository.GetActiveByUserIdAsync(
            user.Id,
            now,
            ct);

        foreach (var other in otherActiveCodes.Where(x => x.Id != recovery.Id))
        {
            other.IsActive = false;
            other.UsedAt = now;
            _unitOfWork.Update(other);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return new PasswordRecoveryMessageResponse
        {
            Message = _messages.PasswordResetSuccess,
            ExpiresInMinutes = 0
        };
    }

    private async Task<Outcome<PasswordRecoveryMessageResponse, HttpBadOutcome>> SendRecoveryCodeAsync(
        ForgotPasswordRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var response = new PasswordRecoveryMessageResponse
        {
            Message = _messages.PasswordRecoveryEmailSent,
            ExpiresInMinutes = PasswordRecoveryConstants.ExpiryMinutes
        };

        var user = await _userInfoRepository.GetByEmailAsync(email, ct);
        if (user is null || !user.IsActive)
        {
            return response;
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.PasswordResetNotAvailable);
        }

        var now = DateTime.UtcNow;
        var (plainCode, codeHash) = await CreateUniqueRecoveryCodeAsync(ct);

        var existingCodes = await _passwordRecoveryCodeRepository.GetActiveByUserIdAsync(user.Id, now, ct);
        foreach (var existing in existingCodes)
        {
            existing.IsActive = false;
            _unitOfWork.Update(existing);
        }

        var recovery = new PasswordRecoveryCode
        {
            UserInfoId = user.Id,
            CodeHash = codeHash,
            ExpiresAt = now.AddMinutes(PasswordRecoveryConstants.ExpiryMinutes),
            CreatedAt = now,
            IsActive = true
        };

        await _unitOfWork.AddAsync(recovery, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _emailSender.SendAsync(
            user.Email,
            _messages.RecoveryCodeEmailSubject,
            string.Format(
                CultureInfo.InvariantCulture,
                _messages.RecoveryCodeEmailBody,
                plainCode,
                PasswordRecoveryConstants.ExpiryMinutes),
            ct);

        return response;
    }

    private async Task<PasswordRecoveryCode?> FindActiveRecoveryAsync(
        string email,
        string code,
        CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedCode = code.Trim();
        var codeHash = HashRecoveryCode(normalizedCode);

        return await _passwordRecoveryCodeRepository.GetActiveByEmailAndCodeHashAsync(
            normalizedEmail,
            codeHash,
            DateTime.UtcNow,
            ct);
    }

    private async Task<(string PlainCode, string CodeHash)> CreateUniqueRecoveryCodeAsync(
        CancellationToken ct)
    {
        for (var attempt = 0; attempt < PasswordRecoveryConstants.MaxGenerationAttempts; attempt++)
        {
            var plainCode = GenerateNumericCode();
            var codeHash = HashRecoveryCode(plainCode);

            if (!await _passwordRecoveryCodeRepository.ExistsActiveCodeHashAsync(codeHash, ct))
            {
                return (plainCode, codeHash);
            }
        }

        throw new InvalidOperationException("Could not generate a unique recovery code.");
    }

    private static string GenerateNumericCode()
    {
        var maxValue = (int)Math.Pow(10, PasswordRecoveryConstants.CodeLength);
        var value = RandomNumberGenerator.GetInt32(0, maxValue);
        return value.ToString($"D{PasswordRecoveryConstants.CodeLength}", CultureInfo.InvariantCulture);
    }

    private static string HashRecoveryCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
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

    private static LoginUserDto MapUser(UserInfo user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        CurrencyCode = user.CurrencyCode
    };
}
