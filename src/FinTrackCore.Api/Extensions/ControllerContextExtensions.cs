using FinTrackCore.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinTrackCore.Api.Extensions;

public static class ControllerContextExtensions
{
    public static IActionResult SendResponse(
        this ControllerContext context,
        int statusCode,
        string? message = null,
        object? data = null,
        ApiMeta? meta = null)
    {
        if (statusCode == StatusCodes.Status204NoContent)
        {
            return new NoContentResult();
        }

        var body = new ApiResponse<object?>
        {
            Success = statusCode is >= 200 and < 300,
            StatusCode = statusCode,
            Message = message ?? string.Empty,
            Data = data,
            Meta = meta
        };

        return new ObjectResult(body)
        {
            StatusCode = statusCode
        };
    }
}
