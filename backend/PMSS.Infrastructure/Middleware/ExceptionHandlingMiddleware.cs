using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using System.Net;
using System.Text.Json;

namespace PMSS.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                code = HttpStatusCode.BadRequest;
                _logger.LogWarning(exception, "Bad request error occurred: {Message}", exception.Message);
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                _logger.LogWarning(exception, "Unauthorized access: {Message}", exception.Message);
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                break;
            default:
                _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
                break;
        }

        var response = ApiResponse<object>.ErrorResponse(
            "An error occurred processing your request",
            exception.Message
        );

        result = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}
