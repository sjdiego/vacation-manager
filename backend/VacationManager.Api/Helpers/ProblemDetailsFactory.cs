using Microsoft.AspNetCore.Http;

namespace VacationManager.Api.Helpers;

public static class ProblemDetailsFactory
{
    public static Models.ProblemDetails CreateBadRequest(
        string detail,
        string? title = null,
        string? instance = null,
        Dictionary<string, object>? extensions = null)
    {
        return new Models.ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Title = title ?? "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = detail,
            Instance = instance ?? string.Empty,
            Extensions = extensions
        };
    }

    public static Models.ProblemDetails CreateNotFound(
        string detail,
        string? title = null,
        string? instance = null)
    {
        return new Models.ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            Title = title ?? "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = detail,
            Instance = instance ?? string.Empty
        };
    }

    public static Models.ProblemDetails CreateForbidden(
        string detail,
        string? title = null,
        string? instance = null)
    {
        return new Models.ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            Title = title ?? "Forbidden",
            Status = StatusCodes.Status403Forbidden,
            Detail = detail,
            Instance = instance ?? string.Empty
        };
    }

    public static Models.ProblemDetails CreateConflict(
        string detail,
        string? title = null,
        string? instance = null)
    {
        return new Models.ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            Title = title ?? "Conflict",
            Status = StatusCodes.Status409Conflict,
            Detail = detail,
            Instance = instance ?? string.Empty
        };
    }

    public static Models.ProblemDetails CreateUnauthorized(
        string detail,
        string? title = null,
        string? instance = null)
    {
        return new Models.ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            Title = title ?? "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = detail,
            Instance = instance ?? string.Empty
        };
    }

    public static Models.ProblemDetails CreateInternalServerError(
        string detail,
        string? title = null,
        string? instance = null,
        Dictionary<string, object>? extensions = null)
    {
        return new Models.ProblemDetails
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = title ?? "An error occurred while processing your request",
            Status = StatusCodes.Status500InternalServerError,
            Detail = detail,
            Instance = instance ?? string.Empty,
            Extensions = extensions
        };
    }
}
