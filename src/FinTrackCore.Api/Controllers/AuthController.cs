using FinTrackCore.Application.Features.Auth;
using FinTrackCore.Application.Features.Auth.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinTrackCore.Api.Controllers;

[Route("api/[controller]s")]
public class AuthController(IAuthService authService) : JsonApiControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.LoginAsync(request, ipAddress, cancellationToken);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(
            StatusCodes.Status200OK,
            data!.Message,
            new
            {
                accessToken = data.AccessToken,
                refreshToken = data.RefreshToken,
                expiresIn = data.ExpiresIn,
                user = data.User
            });
    }
}
