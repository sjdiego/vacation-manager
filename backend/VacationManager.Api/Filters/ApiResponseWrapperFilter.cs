using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VacationManager.Api.Models;

namespace VacationManager.Api.Filters;

/// <summary>
/// Action filter to wrap successful responses in a standard format
/// Can be disabled per action with [DisableApiResponseWrapper] attribute
/// </summary>
public class ApiResponseWrapperFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Nothing to do before action executes
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Check if the action has DisableApiResponseWrapper attribute
        var disableWrapper = context.ActionDescriptor.EndpointMetadata
            .OfType<DisableApiResponseWrapperAttribute>()
            .Any();

        if (disableWrapper)
        {
            return;
        }

        // Skip CreatedAtActionResult and CreatedAtRouteResult to preserve Location header
        if (context.Result is CreatedAtActionResult or CreatedAtRouteResult)
        {
            return;
        }

        // Only wrap successful ObjectResult responses
        if (context.Result is ObjectResult objectResult && 
            objectResult.StatusCode is >= 200 and < 300)
        {
            // Don't wrap if already wrapped, if it's a ProblemDetails, or if value is null
            if (objectResult.Value == null || 
                IsAlreadyWrapped(objectResult.Value) || 
                objectResult.Value is ProblemDetails)
            {
                return;
            }

            var wrappedResponse = new ApiResponse<object>
            {
                Success = true,
                Data = objectResult.Value
            };

            context.Result = new ObjectResult(wrappedResponse)
            {
                StatusCode = objectResult.StatusCode
            };
        }
    }

    private static bool IsAlreadyWrapped(object value)
    {
        var type = value.GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
    }
}

/// <summary>
/// Attribute to disable API response wrapping for specific actions
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DisableApiResponseWrapperAttribute : Attribute
{
}
