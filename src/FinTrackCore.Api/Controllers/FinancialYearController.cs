using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Features.FinancialYears;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Api.Controllers;

[Authorize]
[Route("api/[controller]s")]
public class FinancialYearController : JsonApiControllerBase
{
    private readonly IFinancialYearService _financialYearService;
    private readonly MessageSettings _messages;

    public FinancialYearController(
        IFinancialYearService financialYearService,
        IOptions<MessageSettings> messageOptions)
    {
        _financialYearService = financialYearService;
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

        var result = await _financialYearService.GetAllAsync(currentUserId.Value, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _financialYearService.GetCurrentAsync(currentUserId.Value, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _financialYearService.GetByIdAsync(id, currentUserId.Value, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }
}
