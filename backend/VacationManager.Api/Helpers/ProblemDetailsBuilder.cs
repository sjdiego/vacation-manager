using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace VacationManager.Api.Helpers;

/// <summary>
/// Fluent builder for creating RFC 7807 Problem Details responses
/// </summary>
public class ProblemDetailsBuilder
{
    private string? _type;
    private string? _title;
    private int? _status;
    private string? _detail;
    private string? _instance;
    private Dictionary<string, object?>? _extensions;

    private ProblemDetailsBuilder() { }

    public static ProblemDetailsBuilder New() => new();

    public static ProblemDetailsBuilder BadRequest() => new ProblemDetailsBuilder()
        .WithType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1")
        .WithTitle("Bad Request")
        .WithStatus(StatusCodes.Status400BadRequest);

    public static ProblemDetailsBuilder NotFound() => new ProblemDetailsBuilder()
        .WithType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4")
        .WithTitle("Not Found")
        .WithStatus(StatusCodes.Status404NotFound);

    public static ProblemDetailsBuilder Forbidden() => new ProblemDetailsBuilder()
        .WithType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3")
        .WithTitle("Forbidden")
        .WithStatus(StatusCodes.Status403Forbidden);

    public static ProblemDetailsBuilder Conflict() => new ProblemDetailsBuilder()
        .WithType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8")
        .WithTitle("Conflict")
        .WithStatus(StatusCodes.Status409Conflict);

    public static ProblemDetailsBuilder Unauthorized() => new ProblemDetailsBuilder()
        .WithType("https://datatracker.ietf.org/doc/html/rfc7235#section-3.1")
        .WithTitle("Unauthorized")
        .WithStatus(StatusCodes.Status401Unauthorized);

    public static ProblemDetailsBuilder InternalServerError() => new ProblemDetailsBuilder()
        .WithType("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
        .WithTitle("An error occurred while processing your request")
        .WithStatus(StatusCodes.Status500InternalServerError);

    public ProblemDetailsBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public ProblemDetailsBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ProblemDetailsBuilder WithStatus(int status)
    {
        _status = status;
        return this;
    }

    public ProblemDetailsBuilder WithDetail(string detail)
    {
        _detail = detail;
        return this;
    }

    public ProblemDetailsBuilder WithInstance(string instance)
    {
        _instance = instance;
        return this;
    }

    public ProblemDetailsBuilder WithExtension(string key, object? value)
    {
        _extensions ??= new Dictionary<string, object?>();
        _extensions[key] = value;
        return this;
    }

    public ProblemDetailsBuilder WithTraceId(string? traceId = null)
    {
        if (!string.IsNullOrEmpty(traceId))
        {
            return WithExtension("traceId", traceId);
        }

        var currentTraceId = Activity.Current?.Id;
        if (!string.IsNullOrEmpty(currentTraceId))
        {
            return WithExtension("traceId", currentTraceId);
        }

        return this;
    }

    public ProblemDetailsBuilder WithTraceId(HttpContext context)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        return WithExtension("traceId", traceId);
    }

    public ProblemDetailsBuilder WithValidationErrors(Dictionary<string, object> errors)
    {
        return WithExtension("errors", errors);
    }

    public ProblemDetails Build()
    {
        var problemDetails = new ProblemDetails
        {
            Type = _type,
            Title = _title,
            Status = _status,
            Detail = _detail,
            Instance = _instance
        };

        if (_extensions != null && _extensions.Count > 0)
        {
            problemDetails.Extensions = _extensions;
        }

        return problemDetails;
    }
}
