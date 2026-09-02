using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Features.UserInfos;
using FinTrackCore.Application.Features.UserInfos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Api.Controllers;

[Authorize]
[Route("api/[controller]s")]
public class UserInfoController : JsonApiControllerBase
{
    private readonly IUserInfoService _userInfoService;
    private readonly MessageSettings _messages;

    public UserInfoController(
        IUserInfoService userInfoService,
        IOptions<MessageSettings> messageOptions)
    {
        _userInfoService = userInfoService;
        _messages = messageOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserInfoRequest request,
        CancellationToken ct)
    {
        var result = await _userInfoService.CreateAsync(request, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(
            StatusCodes.Status201Created,
            data!.Message,
            new { id = data.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateUserInfoRequest request,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        if (currentUserId.Value != id)
        {
            return SendResponse(StatusCodes.Status403Forbidden, _messages.Forbidden);
        }

        var result = await _userInfoService.UpdateAsync(id, request, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(
            StatusCodes.Status200OK,
            data!.Message,
            new { id = data.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        long id,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        if (currentUserId.Value != id)
        {
            return SendResponse(StatusCodes.Status403Forbidden, _messages.Forbidden);
        }

        var result = await _userInfoService.GetByIdAsync(id, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(
            StatusCodes.Status200OK,
            string.Empty,
            data);
    }
}
