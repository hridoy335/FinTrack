using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Infrastructure.Services;

public sealed class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleAuthSettings _settings;

    public GoogleAuthService(IOptions<GoogleAuthSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<GoogleUserProfile?> ValidateIdTokenAsync(
        string idToken,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_settings.ClientId))
        {
            return null;
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_settings.ClientId]
                });

            if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Email))
            {
                return null;
            }

            return new GoogleUserProfile
            {
                Subject = payload.Subject,
                Email = payload.Email.Trim().ToLowerInvariant(),
                GivenName = payload.GivenName,
                FamilyName = payload.FamilyName,
                EmailVerified = payload.EmailVerified
            };
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
