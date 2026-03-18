using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PMSS.Application.DTOs.Common;
using System.Net;
using System.Text.Json;

namespace PMSS.Infrastructure.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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
            _env.IsDevelopment() ? exception.Message : "Internal server error"
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
