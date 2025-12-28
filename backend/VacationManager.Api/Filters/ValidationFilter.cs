using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VacationManager.Api.Helpers;

namespace VacationManager.Api.Filters;

/// <summary>
/// Action filter that validates ModelState and returns Problem Details for validation errors.
/// Handles validation errors from Data Annotations and model binding.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray() ?? Array.Empty<string>()
                );

            var traceId = ProblemDetailsFactory.GetTraceId(context.HttpContext);
            var extensions = new Dictionary<string, object>
            {
                ["errors"] = errors
            };

            var problemDetails = ProblemDetailsFactory.CreateBadRequest(
                "One or more validation errors occurred.",
                title: "Validation Failed",
                instance: context.HttpContext.Request.Path,
                extensions: extensions,
                traceId: traceId);

            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }

        await next();
    }
}
