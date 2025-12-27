namespace VacationManager.Api.Models;

/// <summary>
/// Standard API response wrapper for successful operations
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, object>? Meta { get; set; }

    public ApiResponse()
    {
    }

    public ApiResponse(T data, string? message = null)
    {
        Data = data;
        Message = message;
    }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>(data, message);
    }

    public static ApiResponse<T> Created(T data, string? message = null)
    {
        return new ApiResponse<T>(data, message ?? "Resource created successfully");
    }
}

/// <summary>
/// RFC 7807 Problem Details for error responses
/// https://datatracker.ietf.org/doc/html/rfc7807
/// </summary>
public class ProblemDetails
{
    /// <summary>
    /// A URI reference that identifies the problem type
    /// </summary>
    public string Type { get; set; } = "about:blank";

    /// <summary>
    /// A short, human-readable summary of the problem type
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP status code
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// A URI reference that identifies the specific occurrence
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    /// Additional properties for extensibility
    /// </summary>
    public Dictionary<string, object>? Extensions { get; set; }

    /// <summary>
    /// Validation errors (if applicable)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }
}
