using FinTrackCore.Application.Features.Auth.Models;
using SharpOutcome;
using SharpOutcome.Helpers;

namespace FinTrackCore.Application.Features.Auth;

public interface IAuthService
{
    Task<Outcome<LoginResponse, HttpBadOutcome>> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
