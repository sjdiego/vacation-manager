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
    public async Task ValidationFilter_WithInvalidModelState_IncludesErrorDetails()
    {
        // Arrange
        _context.ModelState.AddModelError("Email", "Email is required");
        _context.ModelState.AddModelError("Email", "Invalid email format");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        var badRequestResult = (BadRequestObjectResult)_context.Result!;
        var responseValue = badRequestResult.Value;
        
        Assert.NotNull(responseValue);
        var errors = responseValue!.GetType().GetProperty("errors")?.GetValue(responseValue);
        Assert.NotNull(errors);
    }

    [Fact]
    public async Task ValidationFilter_WithInvalidModelState_IncludesMessage()
    {
        // Arrange
        _context.ModelState.AddModelError("Field", "Error message");

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        var badRequestResult = (BadRequestObjectResult)_context.Result!;
        var responseValue = badRequestResult.Value;
        
        var message = responseValue!.GetType().GetProperty("message")?.GetValue(responseValue);
        Assert.Equal("Validation failed", message);
    }

    [Fact]
    public async Task ValidationFilter_WithInvalidModelState_IncludesTimestamp()
    {
        // Arrange
        _context.ModelState.AddModelError("Field", "Error");
        var beforeFilter = DateTime.UtcNow;

        // Act
        await _filter.OnActionExecutionAsync(_context, _nextDelegate);

        // Assert
        var badRequestResult = (BadRequestObjectResult)_context.Result!;
        var responseValue = badRequestResult.Value;
        var timestamp = responseValue!.GetType().GetProperty("timestamp")?.GetValue(responseValue);
        
        Assert.NotNull(timestamp);
        Assert.IsType<DateTime>(timestamp);
        Assert.True((DateTime)timestamp >= beforeFilter);
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
        var responseValue = badRequestResult.Value;
        
        var errors = responseValue!.GetType().GetProperty("errors")?.GetValue(responseValue);
        Assert.NotNull(errors);
        
        var errorDict = (Dictionary<string, string[]>)errors!;
        Assert.Equal(3, errorDict.Count);
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
        var responseValue = badRequestResult.Value;
        var errors = (Dictionary<string, string[]>)responseValue!.GetType().GetProperty("errors")?.GetValue(responseValue)!;
        
        Assert.NotNull(errors);
        Assert.Single(errors);
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
