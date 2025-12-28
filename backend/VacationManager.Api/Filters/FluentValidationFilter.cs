using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;
using VacationManager.Api.Helpers;

namespace VacationManager.Api.Filters;

/// <summary>
/// Action filter that validates request DTOs using FluentValidation validators.
/// Returns 400 Bad Request with validation errors if validation fails.
/// </summary>
public class FluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null)
                continue;

            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            
            if (_serviceProvider.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var validationResult = await validator.ValidateAsync(validationContext);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
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
            }
        }

        await next();
    }
}
