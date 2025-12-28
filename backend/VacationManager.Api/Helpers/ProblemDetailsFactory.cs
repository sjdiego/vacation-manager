using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace VacationManager.Api.Helpers;

/// <summary>
/// Factory for creating RFC 7807 Problem Details responses
/// Legacy API - consider using ProblemDetailsBuilder for more flexibility
/// </summary>
public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateBadRequest(
        string detail,
        string? title = null,
        string? instance = null,
        Dictionary<string, object>? extensions = null,
        string? traceId = null)
    {
        var builder = ProblemDetailsBuilder.BadRequest()
            .WithDetail(detail);

        if (!string.IsNullOrEmpty(title))
            builder = builder.WithTitle(title);

        if (!string.IsNullOrEmpty(instance))
            builder = builder.WithInstance(instance);

        if (extensions != null)
        {
            foreach (var kvp in extensions)
            {
                builder = builder.WithExtension(kvp.Key, kvp.Value);
            }
        }

        if (!string.IsNullOrEmpty(traceId))
            builder = builder.WithTraceId(traceId);

        return builder.Build();
    }

    public static ProblemDetails CreateNotFound(
        string detail,
        string? title = null,
        string? instance = null,
        string? traceId = null)
    {
        var builder = ProblemDetailsBuilder.NotFound()
            .WithDetail(detail);

        if (!string.IsNullOrEmpty(title))
            builder = builder.WithTitle(title);

        if (!string.IsNullOrEmpty(instance))
            builder = builder.WithInstance(instance);

        if (!string.IsNullOrEmpty(traceId))
            builder = builder.WithTraceId(traceId);

        return builder.Build();
    }

    public static ProblemDetails CreateForbidden(
        string detail,
        string? title = null,
        string? instance = null,
        string? traceId = null)
    {
        var builder = ProblemDetailsBuilder.Forbidden()
            .WithDetail(detail);

        if (!string.IsNullOrEmpty(title))
            builder = builder.WithTitle(title);

        if (!string.IsNullOrEmpty(instance))
            builder = builder.WithInstance(instance);

        if (!string.IsNullOrEmpty(traceId))
            builder = builder.WithTraceId(traceId);

        return builder.Build();
    }

    public static ProblemDetails CreateConflict(
        string detail,
        string? title = null,
        string? instance = null,
        string? traceId = null)
    {
        var builder = ProblemDetailsBuilder.Conflict()
            .WithDetail(detail);

        if (!string.IsNullOrEmpty(title))
            builder = builder.WithTitle(title);

        if (!string.IsNullOrEmpty(instance))
            builder = builder.WithInstance(instance);

        if (!string.IsNullOrEmpty(traceId))
            builder = builder.WithTraceId(traceId);

        return builder.Build();
    }

    public static ProblemDetails CreateUnauthorized(
        string detail,
        string? title = null,
        string? instance = null,
        string? traceId = null)
    {
        var builder = ProblemDetailsBuilder.Unauthorized()
            .WithDetail(detail);

        if (!string.IsNullOrEmpty(title))
            builder = builder.WithTitle(title);

        if (!string.IsNullOrEmpty(instance))
            builder = builder.WithInstance(instance);

        if (!string.IsNullOrEmpty(traceId))
            builder = builder.WithTraceId(traceId);

        return builder.Build();
    }

    public static ProblemDetails CreateInternalServerError(
        string detail,
        string? title = null,
        string? instance = null,
        Dictionary<string, object>? extensions = null,
        string? traceId = null)
    {
        var builder = ProblemDetailsBuilder.InternalServerError()
            .WithDetail(detail);

        if (!string.IsNullOrEmpty(title))
            builder = builder.WithTitle(title);

        if (!string.IsNullOrEmpty(instance))
            builder = builder.WithInstance(instance);

        if (extensions != null)
        {
            foreach (var kvp in extensions)
            {
                builder = builder.WithExtension(kvp.Key, kvp.Value);
            }
        }

        if (!string.IsNullOrEmpty(traceId))
            builder = builder.WithTraceId(traceId);

        return builder.Build();
    }

    /// <summary>
    /// Gets the trace ID from the current activity or HTTP context
    /// </summary>
    public static string? GetTraceId(HttpContext? context = null)
    {
        return Activity.Current?.Id ?? context?.TraceIdentifier;
    }
}
