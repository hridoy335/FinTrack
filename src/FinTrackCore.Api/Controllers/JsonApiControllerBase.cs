using System.Security.Claims;
using FinTrackCore.Api.Extensions;
using FinTrackCore.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using SharpOutcome.Helpers.Contracts;
using SharpOutcome.Helpers.Enums;

namespace FinTrackCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public abstract class JsonApiControllerBase : ControllerBase
{
    protected IActionResult SendResponse(
        int statusCode,
        string? message = null,
        object? data = null,
        ApiMeta? meta = null)
    {
        return ControllerContext.SendResponse(statusCode, message, data, meta);
    }

    protected IActionResult SendPagedResponse<T>(
        IReadOnlyList<T> data,
        long totalData,
        int pageSize,
        string? message = null,
        int statusCode = StatusCodes.Status200OK)
    {
        if (pageSize <= 0)
        {
            pageSize = 1;
        }

        var totalPage = (long)Math.Ceiling(totalData / (decimal)pageSize);

        return SendResponse(
            statusCode,
            message,
            data,
            new ApiMeta
            {
                TotalData = totalData,
                TotalPage = totalPage
            });
    }

    protected IActionResult HttpBadOutcomeResponse(IBadOutcome<HttpBadOutcomeTag> error)
    {
        return error.Tag switch
        {
            HttpBadOutcomeTag.Conflict => SendResponse(StatusCodes.Status409Conflict, error.Reason),
            HttpBadOutcomeTag.BadRequest => SendResponse(StatusCodes.Status400BadRequest, error.Reason),
            HttpBadOutcomeTag.NotFound => SendResponse(StatusCodes.Status404NotFound, error.Reason),
            HttpBadOutcomeTag.Unauthorized => SendResponse(StatusCodes.Status401Unauthorized, error.Reason),
            HttpBadOutcomeTag.Forbidden => SendResponse(StatusCodes.Status403Forbidden, error.Reason),
            HttpBadOutcomeTag.NotImplemented => SendResponse(StatusCodes.Status501NotImplemented, error.Reason),
            HttpBadOutcomeTag.RequestTimeout => SendResponse(StatusCodes.Status408RequestTimeout, error.Reason),
            HttpBadOutcomeTag.InternalServerError => SendResponse(StatusCodes.Status500InternalServerError, error.Reason),
            _ => SendResponse(StatusCodes.Status400BadRequest, error.Reason)
        };
    }

    protected IActionResult BadOutcomeResponse(IBadOutcome error)
    {
        return error.Tag switch
        {
            BadOutcomeTag.Conflict => SendResponse(StatusCodes.Status409Conflict, error.Reason),
            BadOutcomeTag.BadRequest => SendResponse(StatusCodes.Status400BadRequest, error.Reason),
            BadOutcomeTag.NotFound => SendResponse(StatusCodes.Status404NotFound, error.Reason),
            BadOutcomeTag.Unauthorized => SendResponse(StatusCodes.Status401Unauthorized, error.Reason),
            BadOutcomeTag.Forbidden => SendResponse(StatusCodes.Status403Forbidden, error.Reason),
            BadOutcomeTag.Timeout => SendResponse(StatusCodes.Status408RequestTimeout, error.Reason),
            BadOutcomeTag.Duplicate => SendResponse(StatusCodes.Status409Conflict, error.Reason),
            BadOutcomeTag.Unknown => SendResponse(StatusCodes.Status500InternalServerError, error.Reason),
            _ => SendResponse(StatusCodes.Status400BadRequest, error.Reason)
        };
    }

    protected long? GetCurrentLoggedInUserId(string? locator = null)
    {
        var key = locator ?? ClaimTypes.NameIdentifier;
        var id = User.FindFirst(x => x.Type == key)?.Value;

        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return long.TryParse(id, out var result) ? result : null;
    }
}
