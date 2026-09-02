using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Constants;
using FinTrackCore.Application.Features.Transactions;
using FinTrackCore.Application.Features.Transactions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Api.Controllers;

[Authorize]
[Route("api/[controller]s")]
public class TransactionController : JsonApiControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly MessageSettings _messages;

    public TransactionController(
        ITransactionService transactionService,
        IOptions<MessageSettings> messageOptions)
    {
        _transactionService = transactionService;
        _messages = messageOptions.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] TransactionListQuery query,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var pageSize = query.PageSize <= 0
            ? PaginationConstants.DefaultPageSize
            : Math.Min(query.PageSize, PaginationConstants.MaxPageSize);

        var result = await _transactionService.GetAllAsync(currentUserId.Value, query, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);

        return SendPagedResponse(
            data!.Items,
            data.TotalCount,
            pageSize,
            string.Empty);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _transactionService.GetByIdAsync(id, currentUserId.Value, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _transactionService.CreateAsync(currentUserId.Value, request, ct);

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
}
