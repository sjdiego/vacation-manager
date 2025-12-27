using Microsoft.AspNetCore.Mvc;
using VacationManager.Api.Helpers;

namespace VacationManager.Api.Extensions;

/// <summary>
/// Extension methods for creating standardized Problem Details responses
/// </summary>
public static class ControllerBaseExtensions
{
    public static BadRequestObjectResult BadRequestProblem(
        this ControllerBase controller,
        string detail,
        string? title = null,
        Dictionary<string, object>? extensions = null)
    {
        var problemDetails = ProblemDetailsFactory.CreateBadRequest(
            detail,
            title,
            controller.HttpContext?.Request.Path,
            extensions);

        return new BadRequestObjectResult(problemDetails);
    }

    public static NotFoundObjectResult NotFoundProblem(
        this ControllerBase controller,
        string detail,
        string? title = null)
    {
        var problemDetails = ProblemDetailsFactory.CreateNotFound(
            detail,
            title,
            controller.HttpContext?.Request.Path);

        return new NotFoundObjectResult(problemDetails);
    }

    public static ObjectResult ForbiddenProblem(
        this ControllerBase controller,
        string detail,
        string? title = null)
    {
        var problemDetails = ProblemDetailsFactory.CreateForbidden(
            detail,
            title,
            controller.HttpContext?.Request.Path);

        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }

    public static ConflictObjectResult ConflictProblem(
        this ControllerBase controller,
        string detail,
        string? title = null)
    {
        var problemDetails = ProblemDetailsFactory.CreateConflict(
            detail,
            title,
            controller.HttpContext?.Request.Path);

        return new ConflictObjectResult(problemDetails);
    }
}
