using FinTrackCore.Application.Features.Auth.Models;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.Auth;

public interface IAuthService
{
    Task<Outcome<LoginResponse, HttpBadOutcome>> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken ct);

    Task<Outcome<LoginResponse, HttpBadOutcome>> GoogleAsync(
        GoogleAuthRequest request,
        string? ipAddress,
        CancellationToken ct);

    Task<Outcome<LoginResponse, HttpBadOutcome>> RefreshAsync(
        RefreshRequest request,
        string? ipAddress,
        CancellationToken ct);

    Task<Outcome<LogoutResponse, HttpBadOutcome>> LogoutAsync(
        LogoutRequest request,
        CancellationToken ct);
}
