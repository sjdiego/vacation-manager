using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VacationManager.Api.Helpers;

namespace VacationManager.Api.Middleware;

/// <summary>
/// Middleware to handle exceptions globally and return RFC 7807 Problem Details
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = ProblemDetailsFactory.GetTraceId(context);
        ProblemDetails problemDetails;

        switch (exception)
        {
            case ArgumentException argEx:
                problemDetails = ProblemDetailsFactory.CreateBadRequest(
                    argEx.Message,
                    instance: context.Request.Path,
                    traceId: traceId);
                break;

            case UnauthorizedAccessException:
                problemDetails = ProblemDetailsFactory.CreateUnauthorized(
                    "You are not authorized to access this resource",
                    instance: context.Request.Path,
                    traceId: traceId);
                break;

            case KeyNotFoundException:
                problemDetails = ProblemDetailsFactory.CreateNotFound(
                    exception.Message,
                    instance: context.Request.Path,
                    traceId: traceId);
                break;

            default:
                var detail = _env.IsDevelopment() 
                    ? exception.Message 
                    : "An unexpected error occurred";
                
                var extensions = _env.IsDevelopment()
                    ? new Dictionary<string, object>
                    {
                        ["stackTrace"] = exception.StackTrace ?? string.Empty,
                        ["exceptionType"] = exception.GetType().Name
                    }
                    : null;

                problemDetails = ProblemDetailsFactory.CreateInternalServerError(
                    detail,
                    instance: context.Request.Path,
                    extensions: extensions,
                    traceId: traceId);
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsJsonAsync(problemDetails, options);
    }
}
