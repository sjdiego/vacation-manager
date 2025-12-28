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
