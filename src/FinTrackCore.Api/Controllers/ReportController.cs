using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Features.Reports;
using FinTrackCore.Application.Features.Reports.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Api.Controllers;

[Authorize]
[Route("api/[controller]s")]
public class ReportController : JsonApiControllerBase
{
    private readonly IReportService _reportService;
    private readonly MessageSettings _messages;

    public ReportController(
        IReportService reportService,
        IOptions<MessageSettings> messageOptions)
    {
        _reportService = reportService;
        _messages = messageOptions.Value;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DashboardReportQuery query,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _reportService.GetDashboardAsync(currentUserId.Value, query, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }

    [HttpGet("cashflow")]
    public async Task<IActionResult> GetCashflow(
        [FromQuery] CashflowReportQuery query,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _reportService.GetCashflowAsync(currentUserId.Value, query, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance(
        [FromQuery] BalanceReportQuery query,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _reportService.GetBalanceAsync(currentUserId.Value, query, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }

    [HttpGet("account-statement")]
    public async Task<IActionResult> GetAccountStatement(
        [FromQuery] AccountStatementQuery query,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _reportService.GetAccountStatementAsync(currentUserId.Value, query, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }

    [HttpGet("monthly-cashflow")]
    public async Task<IActionResult> GetMonthlyCashflow(
        [FromQuery] MonthlyCashflowReportQuery query,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _reportService.GetMonthlyCashflowAsync(currentUserId.Value, query, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }
}
