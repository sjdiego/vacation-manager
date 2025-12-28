using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using VacationManager.Api.Filters;
using VacationManager.Api.Models;

namespace VacationManager.Tests.Api.Filters;

public class ApiResponseWrapperFilterTests
{
    private readonly ApiResponseWrapperFilter _filter;

    public ApiResponseWrapperFilterTests()
    {
        _filter = new ApiResponseWrapperFilter();
    }

    private ActionExecutedContext CreateContext(IActionResult? result = null, params object[] endpointMetadata)
    {
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = endpointMetadata.ToList()
        };
        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

        return new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            null!)
        {
            Result = result
        };
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithSuccessfulObjectResult_WrapsResponse()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var objectResult = new ObjectResult(data) { StatusCode = StatusCodes.Status200OK };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        var wrappedResponse = Assert.IsType<ApiResponse<object>>(result.Value);
        Assert.True(wrappedResponse.Success);
        Assert.Equal(data, wrappedResponse.Data);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithDisableAttribute_DoesNotWrapResponse()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var objectResult = new ObjectResult(data) { StatusCode = StatusCodes.Status200OK };
        var context = CreateContext(objectResult, new DisableApiResponseWrapperAttribute());

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(data, result.Value);
        Assert.NotEqual(typeof(ApiResponse<object>), result.Value?.GetType());
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithCreatedAtActionResult_DoesNotWrapResponse()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var createdResult = new CreatedAtActionResult("GetById", "Test", new { id = 1 }, data);
        var context = CreateContext(createdResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<CreatedAtActionResult>(context.Result);
        Assert.Equal(data, result.Value);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithCreatedAtRouteResult_DoesNotWrapResponse()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var createdResult = new CreatedAtRouteResult("GetRoute", new { id = 1 }, data);
        var context = CreateContext(createdResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<CreatedAtRouteResult>(context.Result);
        Assert.Equal(data, result.Value);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithProblemDetails_DoesNotWrapResponse()
    {
        // Arrange
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request"
        };
        var objectResult = new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status400BadRequest };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.IsType<ProblemDetails>(result.Value);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithAlreadyWrappedResponse_DoesNotDoubleWrap()
    {
        // Arrange
        var wrappedData = new ApiResponse<object> { Success = true, Data = new { Id = 1 } };
        var objectResult = new ObjectResult(wrappedData) { StatusCode = StatusCodes.Status200OK };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        var response = Assert.IsType<ApiResponse<object>>(result.Value);
        Assert.Equal(wrappedData, response);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithAlreadyWrappedStringResponse_DoesNotDoubleWrap()
    {
        // Arrange
        var wrappedData = new ApiResponse<string> { Success = true, Data = "Test string" };
        var objectResult = new ObjectResult(wrappedData) { StatusCode = StatusCodes.Status200OK };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.IsType<ApiResponse<string>>(result.Value);
        Assert.Equal(wrappedData, result.Value);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithAlreadyWrappedIntResponse_DoesNotDoubleWrap()
    {
        // Arrange
        var wrappedData = new ApiResponse<int> { Success = true, Data = 42 };
        var objectResult = new ObjectResult(wrappedData) { StatusCode = StatusCodes.Status200OK };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.IsType<ApiResponse<int>>(result.Value);
        Assert.Equal(wrappedData, result.Value);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithAlreadyWrappedListResponse_DoesNotDoubleWrap()
    {
        // Arrange
        var wrappedData = new ApiResponse<List<string>> { Success = true, Data = new List<string> { "a", "b", "c" } };
        var objectResult = new ObjectResult(wrappedData) { StatusCode = StatusCodes.Status200OK };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.IsType<ApiResponse<List<string>>>(result.Value);
        Assert.Equal(wrappedData, result.Value);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithNullValue_DoesNotWrapResponse()
    {
        // Arrange
        var objectResult = new ObjectResult(null) { StatusCode = StatusCodes.Status200OK };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithNonObjectResult_DoesNotWrapResponse()
    {
        // Arrange
        var noContentResult = new NoContentResult();
        var context = CreateContext(noContentResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithErrorStatusCode_DoesNotWrapResponse()
    {
        // Arrange
        var data = new { Error = "Something went wrong" };
        var objectResult = new ObjectResult(data) { StatusCode = StatusCodes.Status500InternalServerError };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(data, result.Value);
        Assert.NotEqual(typeof(ApiResponse<object>), result.Value?.GetType());
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithStatus201_WrapsResponse()
    {
        // Arrange
        var data = new { Id = 1, Name = "Created" };
        var objectResult = new ObjectResult(data) { StatusCode = StatusCodes.Status201Created };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        var wrappedResponse = Assert.IsType<ApiResponse<object>>(result.Value);
        Assert.True(wrappedResponse.Success);
        Assert.Equal(data, wrappedResponse.Data);
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
    }

    [Fact]
    public void ApiResponseWrapperFilter_WithStatus204_DoesNotWrap()
    {
        // Arrange
        var objectResult = new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Null(result.Value);
    }

    [Fact]
    public void ApiResponseWrapperFilter_PreservesStatusCode_WhenWrapping()
    {
        // Arrange
        var data = new { Message = "Accepted" };
        var objectResult = new ObjectResult(data) { StatusCode = StatusCodes.Status202Accepted };
        var context = CreateContext(objectResult);

        // Act
        _filter.OnActionExecuted(context);

        // Assert
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
    }

    [Fact]
    public void ApiResponseWrapperFilter_OnActionExecuting_DoesNothing()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ActionDescriptor();
        var routeData = new RouteData();
        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            null!);

        // Act & Assert - should not throw
        _filter.OnActionExecuting(context);
        Assert.Null(context.Result);
    }
}
