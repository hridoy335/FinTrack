using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Features.TransactionTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Api.Controllers;

[Authorize]
[Route("api/[controller]s")]
public class TransactionTypeController : JsonApiControllerBase
{
    private readonly ITransactionTypeService _transactionTypeService;
    private readonly MessageSettings _messages;

    public TransactionTypeController(
        ITransactionTypeService transactionTypeService,
        IOptions<MessageSettings> messageOptions)
    {
        _transactionTypeService = transactionTypeService;
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

        var result = await _transactionTypeService.GetAllAsync(ct);

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

        var result = await _transactionTypeService.GetByIdAsync(id, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }
}
