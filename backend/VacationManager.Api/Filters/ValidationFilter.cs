using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VacationManager.Api.Filters;

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

            var response = new
            {
                message = "Validation failed",
                errors = errors,
                timestamp = DateTime.UtcNow
            };

            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}
