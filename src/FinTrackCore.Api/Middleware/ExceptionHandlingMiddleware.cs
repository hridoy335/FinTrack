using System.Net;
using System.Text.Json;
using FinTrackCore.Application.Common.Configuration;
using FinTrackCore.Application.Common.Errors;
using FinTrackCore.Application.Common.Exceptions;
using FinTrackCore.Application.Common.Models;
using FinTrackCore.Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace FinTrackCore.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IOptions<MessageSettings> messageOptions)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly MessageSettings _messages = messageOptions.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var (statusCode, _, message, errors) = MapException(exception);

        if (statusCode >= (int)HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
        }
        else
        {
            logger.LogWarning(exception, "Handled exception. TraceId: {TraceId}", traceId);
        }

        var response = new ApiResponse<object?>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Data = errors,
            Meta = null
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private (int StatusCode, string ErrorCode, string Message, IReadOnlyList<string>? Errors) MapException(
        Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                validationException.StatusCode,
                validationException.ErrorCode,
                validationException.Message,
                validationException.Errors),

            AppException appException => (
                appException.StatusCode,
                appException.ErrorCode,
                appException.Message,
                appException.Errors),

            DomainException domainException => (
                AppException.StatusCodes.BadRequest,
                ErrorCodes.DomainError,
                string.IsNullOrWhiteSpace(domainException.Message)
                    ? _messages.DomainError
                    : domainException.Message,
                null),

            UnauthorizedAccessException => (
                AppException.StatusCodes.Unauthorized,
                ErrorCodes.Unauthorized,
                _messages.Unauthorized,
                null),

            KeyNotFoundException keyNotFoundException => (
                AppException.StatusCodes.NotFound,
                ErrorCodes.NotFound,
                string.IsNullOrWhiteSpace(keyNotFoundException.Message)
                    ? _messages.NotFound
                    : keyNotFoundException.Message,
                null),

            OperationCanceledException => (
                StatusCodes.Status499ClientClosedRequest,
                ErrorCodes.RequestCanceled,
                _messages.RequestCanceled,
                null),

            _ => (
                AppException.StatusCodes.InternalServerError,
                ErrorCodes.InternalError,
                _messages.InternalError,
                null)
        };
    }
}
