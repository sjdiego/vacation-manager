using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using VacationManager.Api.Filters;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Validators;

namespace VacationManager.Tests.Api.Filters;

public class FluentValidationFilterTests
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationFilterTests()
    {
        var services = new ServiceCollection();
        services.AddScoped<IValidator<CreateVacationDto>, CreateVacationDtoValidator>();
        services.AddScoped<IValidator<UpdateVacationDto>, UpdateVacationDtoValidator>();
        _serviceProvider = services.BuildServiceProvider();
    }

    private ActionExecutingContext CreateContext(object? argument = null)
    {
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor();
        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

        var actionArguments = new Dictionary<string, object?>();
        if (argument != null)
        {
            actionArguments["dto"] = argument;
        }

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments,
            null!);
    }

    [Fact]
    public async Task FluentValidationFilter_WithValidDto_AllowsRequestToProceed()
    {
        // Arrange
        var filter = new FluentValidationFilter(_serviceProvider);
        var dto = new CreateVacationDto
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(5),
            Type = VacationType.Vacation,
            Notes = "Summer vacation"
        };
        var context = CreateContext(dto);
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(
                context,
                new List<IFilterMetadata>(),
                null!));
        };

        // Act
        await filter.OnActionExecutionAsync(context, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task FluentValidationFilter_WithInvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var filter = new FluentValidationFilter(_serviceProvider);
        var dto = new CreateVacationDto
        {
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today, // End date before start date
            Type = VacationType.Vacation,
            Notes = "Invalid vacation"
        };
        var context = CreateContext(dto);
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(
                context,
                new List<IFilterMetadata>(),
                null!));
        };

        // Act
        await filter.OnActionExecutionAsync(context, next);

        // Assert
        Assert.False(nextCalled);
        Assert.NotNull(context.Result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal("Validation Failed", problemDetails.Title);
        Assert.True(problemDetails.Extensions?.ContainsKey("errors"));
    }

    [Fact]
    public async Task FluentValidationFilter_WithMissingStartDate_ReturnsBadRequest()
    {
        // Arrange
        var filter = new FluentValidationFilter(_serviceProvider);
        var dto = new CreateVacationDto
        {
            StartDate = default, // Missing start date
            EndDate = DateTime.Today.AddDays(5),
            Type = VacationType.Vacation
        };
        var context = CreateContext(dto);
        ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(
            context,
            new List<IFilterMetadata>(),
            null!));

        // Act
        await filter.OnActionExecutionAsync(context, next);

        // Assert
        Assert.NotNull(context.Result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        
        Assert.NotNull(problemDetails.Extensions);
        Assert.True(problemDetails.Extensions.ContainsKey("errors"));
        var errors = problemDetails.Extensions["errors"] as Dictionary<string, string[]>;
        Assert.NotNull(errors);
        Assert.True(errors!.ContainsKey("StartDate"));
    }

    [Fact]
    public async Task FluentValidationFilter_WithNotesTooLong_ReturnsBadRequest()
    {
        // Arrange
        var filter = new FluentValidationFilter(_serviceProvider);
        var dto = new CreateVacationDto
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(5),
            Type = VacationType.Vacation,
            Notes = new string('x', 1001) // Exceeds 1000 character limit
        };
        var context = CreateContext(dto);
        ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(
            context,
            new List<IFilterMetadata>(),
            null!));

        // Act
        await filter.OnActionExecutionAsync(context, next);

        // Assert
        Assert.NotNull(context.Result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        
        var errors = problemDetails.Extensions?["errors"] as Dictionary<string, string[]>;
        Assert.NotNull(errors);
        Assert.True(errors!.ContainsKey("Notes"));
        Assert.Contains("cannot exceed 1000 characters", errors["Notes"][0]);
    }

    [Fact]
    public async Task FluentValidationFilter_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var filter = new FluentValidationFilter(_serviceProvider);
        var dto = new CreateVacationDto
        {
            StartDate = default, // Missing
            EndDate = default, // Missing
            Type = (VacationType)999, // Invalid enum
            Notes = new string('x', 1001) // Too long
        };
        var context = CreateContext(dto);
        ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(
            context,
            new List<IFilterMetadata>(),
            null!));

        // Act
        await filter.OnActionExecutionAsync(context, next);

        // Assert
        Assert.NotNull(context.Result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        
        var errors = problemDetails.Extensions?["errors"] as Dictionary<string, string[]>;
        Assert.NotNull(errors);
        Assert.True(errors!.Count >= 3); // At least StartDate, EndDate, Type, Notes errors
    }

    [Fact]
    public async Task FluentValidationFilter_WithNoValidator_AllowsRequestToProceed()
    {
        // Arrange
        var filter = new FluentValidationFilter(_serviceProvider);
        var dto = new { SomeProperty = "value" }; // No validator registered for this type
        var context = CreateContext(dto);
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(
                context,
                new List<IFilterMetadata>(),
                null!));
        };

        // Act
        await filter.OnActionExecutionAsync(context, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task FluentValidationFilter_WithNullArgument_AllowsRequestToProceed()
    {
        // Arrange
        var filter = new FluentValidationFilter(_serviceProvider);
        var context = CreateContext(null);
        var nextCalled = false;
        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(
                context,
                new List<IFilterMetadata>(),
                null!));
        };

        // Act
        await filter.OnActionExecutionAsync(context, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task FluentValidationFilter_WithUpdateDto_ValidatesCorrectly()
    {
        // Arrange
        var filter = new FluentValidationFilter(_serviceProvider);
        var dto = new UpdateVacationDto
        {
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today, // Invalid: end before start
            Type = VacationType.Vacation
        };
        var context = CreateContext(dto);
        ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(
            context,
            new List<IFilterMetadata>(),
            null!));

        // Act
        await filter.OnActionExecutionAsync(context, next);

        // Assert
        Assert.NotNull(context.Result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(context.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Equal("Validation Failed", problemDetails.Title);
    }
}
