using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Features.AccountTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Api.Controllers;

[Authorize]
[Route("api/[controller]s")]
public class AccountTypeController : JsonApiControllerBase
{
    private readonly IAccountTypeService _accountTypeService;
    private readonly MessageSettings _messages;

    public AccountTypeController(
        IAccountTypeService accountTypeService,
        IOptions<MessageSettings> messageOptions)
    {
        _accountTypeService = accountTypeService;
        _messages = messageOptions.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _accountTypeService.GetAllAsync(ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }
}
