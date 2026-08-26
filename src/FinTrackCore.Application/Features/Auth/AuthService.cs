using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Features.Auth.Models;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly MessageSettings _messages;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserInfoRepository userInfoRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        IOptions<MessageSettings> messageOptions,
        IOptions<JwtSettings> jwtOptions)
    {
        _userInfoRepository = userInfoRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _messages = messageOptions.Value;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<Outcome<LoginResponse, HttpBadOutcome>> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.BadRequest, _messages.ValidationFailed);
        }

        var loginKey = request.UserNameOrEmail.Trim();
        var user = await _userInfoRepository.GetByUserNameAsync(loginKey, cancellationToken);

        if (user is null)
        {
            user = await _userInfoRepository.GetByEmailAsync(loginKey.ToLowerInvariant(), cancellationToken);
        }

        if (user is null || !user.IsActive || !_passwordService.Verify(request.Password, user.PasswordHash))
        {
            return new HttpBadOutcome(HttpBadOutcomeTag.Unauthorized, _messages.LoginFailed);
        }

        var accessToken = _jwtTokenService.CreateAccessToken(user);
        var (plainRefreshToken, refreshTokenHash, refreshExpiresAt) = _jwtTokenService.CreateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserInfoId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        await _unitOfWork.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = plainRefreshToken,
            ExpiresIn = _jwtSettings.AccessTokenMinutes * 60,
            Message = _messages.LoginSuccess,
            User = new LoginUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CurrencyCode = user.CurrencyCode
            }
        };
    }
}
