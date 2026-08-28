using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Features.Reports;
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
        [FromQuery] long? financialYearId,
        CancellationToken ct)
    {
        var currentUserId = GetCurrentLoggedInUserId();
        if (currentUserId is null)
        {
            return SendResponse(StatusCodes.Status401Unauthorized, _messages.Unauthorized);
        }

        var result = await _reportService.GetDashboardAsync(currentUserId.Value, financialYearId, ct);

        if (result.TryPickBadOutcome(out var error))
        {
            return HttpBadOutcomeResponse(error);
        }

        _ = result.TryPickGoodOutcome(out var data);
        return SendResponse(StatusCodes.Status200OK, string.Empty, data);
    }
}
