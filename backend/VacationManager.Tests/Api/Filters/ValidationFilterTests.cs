using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using VacationManager.Api.Filters;

namespace VacationManager.Tests.Api.Filters;

public class ValidationFilterTests
{
    private readonly ValidationFilter _filter;
    private readonly ActionExecutingContext _context;
    private readonly ActionExecutionDelegate _nextDelegate;
    private bool _nextCalled;

    public ValidationFilterTests()
    {
        _filter = new ValidationFilter();
        
        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ControllerActionDescriptor { ActionName = "Test", ControllerName = "Test" };
        var routeData = new RouteData();
        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
        
        _context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            null!);

        _nextCalled = false;
        _nextDelegate = async () =>
        {
            _nextCalled = true;
            var executedContext = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                null!);
            return executedContext;
        };
    }

    [Fact]
    public async Task ValidationFilter_WithValidModelState_CallsNext()
    {
        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        Assert.True(_nextCalled);
        Assert.Null(_context.Result);
    }

    [Fact]
    public async Task ValidationFilter_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _context.ModelState.AddModelError("Email", "Email is required");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        Assert.NotNull(_context.Result);
        Assert.IsType<BadRequestObjectResult>(_context.Result);
        Assert.False(_nextCalled);
    }

    [Fact]
    public async Task ValidationFilter_WithInvalidModelState_ReturnsProblemDetails()
    {
        // Arrange
        _context.ModelState.AddModelError("Email", "Email is required");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        var badRequestResult = (BadRequestObjectResult)_context.Result!;
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal("Validation Failed", problemDetails.Title);
        Assert.NotNull(problemDetails.Extensions);
        Assert.True(problemDetails.Extensions!.ContainsKey("errors"));
    }

    [Fact]
    public async Task ValidationFilter_WithInvalidModelState_IncludesErrorDetails()
    {
        // Arrange
        _context.ModelState.AddModelError("Email", "Email is required");
        _context.ModelState.AddModelError("Email", "Invalid email format");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        var badRequestResult = (BadRequestObjectResult)_context.Result!;
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        
        Assert.NotNull(problemDetails.Extensions);
        var errors = problemDetails.Extensions!["errors"] as Dictionary<string, string[]>;
        Assert.NotNull(errors);
        Assert.True(errors!.ContainsKey("Email"));
        Assert.Equal(2, errors["Email"].Length);
    }

    [Fact]
    public async Task ValidationFilter_WithInvalidModelState_IncludesInstance()
    {
        // Arrange
        _context.ModelState.AddModelError("Field", "Error message");
        _context.HttpContext.Request.Path = "/api/v1/vacations";

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        var badRequestResult = (BadRequestObjectResult)_context.Result!;
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        
        Assert.Equal("/api/v1/vacations", problemDetails.Instance);
    }

    [Fact]
    public async Task ValidationFilter_WithInvalidModelState_ReturnsCorrectType()
    {
        // Arrange
        _context.ModelState.AddModelError("Field", "Error message");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        var badRequestResult = (BadRequestObjectResult)_context.Result!;
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1", problemDetails.Type);
        Assert.Equal("One or more validation errors occurred.", problemDetails.Detail);
    }

[Fact]
    public async Task ValidationFilter_WithMultipleFieldErrors_ReturnsAllErrors()
    {
        // Arrange
        _context.ModelState.AddModelError("Email", "Email is required");
        _context.ModelState.AddModelError("Password", "Password is required");
        _context.ModelState.AddModelError("Name", "Name is required");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        Assert.NotNull(_context.Result);
        Assert.IsType<BadRequestObjectResult>(_context.Result);
        var badRequestResult = (BadRequestObjectResult)_context.Result;
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        
        var errors = problemDetails.Extensions!["errors"] as Dictionary<string, string[]>;
        Assert.NotNull(errors);
        Assert.Equal(3, errors!.Count);
    }

    [Fact]
    public async Task ValidationFilter_ReturnsBadRequestStatusCode()
    {
        // Arrange
        _context.ModelState.AddModelError("Field", "Error");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        var badRequestResult = (BadRequestObjectResult)_context.Result!;
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ValidationFilter_WithMultipleErrorsOnSameField_IncludesAll()
    {
        // Arrange
        _context.ModelState.AddModelError("Email", "Email is required");
        _context.ModelState.AddModelError("Email", "Email format is invalid");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        var badRequestResult = (BadRequestObjectResult)_context.Result!;
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        var errors = problemDetails.Extensions!["errors"] as Dictionary<string, string[]>;
        
        Assert.NotNull(errors);
        Assert.Single(errors!);
        Assert.Equal(2, errors["Email"].Length);
    }

    [Fact]
    public async Task ValidationFilter_PreventsRequestFromReachingAction()
    {
        // Arrange
        _context.ModelState.AddModelError("Field", "Error");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        Assert.False(_nextCalled);
        Assert.NotNull(_context.Result);
    }
}
