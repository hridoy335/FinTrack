using FinTrackCore.Application.Features.Auth;
using FinTrackCore.Application.Features.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrackCore.Api.Controllers;

[Route("api/[controller]s")]
public class AuthController : JsonApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request, ipAddress, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendTokenResponse(data!);
    }

    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<IActionResult> Google(
        [FromBody] GoogleAuthRequest request,
        CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.GoogleAsync(request, ipAddress, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendTokenResponse(data!);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshAsync(request, ipAddress, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendTokenResponse(data!);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken ct)
    {
        var result = await _authService.LogoutAsync(request, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(
            StatusCodes.Status200OK,
            data!.Message);
    }

    private IActionResult SendTokenResponse(LoginResponse data)
    {
        return SendResponse(
            StatusCodes.Status200OK,
            data.Message,
            new
            {
                accessToken = data.AccessToken,
                refreshToken = data.RefreshToken,
                expiresIn = data.ExpiresIn,
                user = data.User
            });
    }
}
