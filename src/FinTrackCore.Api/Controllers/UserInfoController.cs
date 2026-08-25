using FinTrackCore.Application.Features.Users;
using FinTrackCore.Application.Features.Users.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinTrackCore.Api.Controllers;

[Route("api/[controller]s")]
public class UserInfoController(IUserInfoService userInfoService) : JsonApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserInfoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userInfoService.CreateAsync(request, cancellationToken);

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

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateUserInfoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userInfoService.UpdateAsync(id, request, cancellationToken);

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

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await userInfoService.GetByIdAsync(id, cancellationToken);

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
