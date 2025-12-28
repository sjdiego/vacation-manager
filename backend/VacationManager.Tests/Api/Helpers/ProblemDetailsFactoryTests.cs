using Xunit;
using Microsoft.AspNetCore.Http;
using VacationManager.Api.Helpers;

namespace VacationManager.Tests.Api.Helpers;

public class ProblemDetailsFactoryTests
{
    [Fact]
    public void CreateBadRequest_WithoutExtensionsOrTraceId_DoesNotAddExtensions()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateBadRequest("Bad request error");

        // Assert
        // ProblemDetails may initialize Extensions as empty, verify it's null or empty
        Assert.True(problemDetails.Extensions == null || problemDetails.Extensions.Count == 0);
    }

    [Fact]
    public void CreateBadRequest_WithTraceId_InitializesExtensionsWithTraceId()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateBadRequest("Bad request error", traceId: "trace-123");

        // Assert
        Assert.NotNull(problemDetails.Extensions);
        Assert.Contains("traceId", problemDetails.Extensions.Keys);
        Assert.Equal("trace-123", problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public void CreateBadRequest_WithExtensions_InitializesExtensionsWithProvidedData()
    {
        // Arrange
        var extensions = new Dictionary<string, object>
        {
            ["customField"] = "customValue"
        };

        // Act
        var problemDetails = ProblemDetailsFactory.CreateBadRequest("Bad request error", extensions: extensions);

        // Assert
        Assert.NotNull(problemDetails.Extensions);
        Assert.Contains("customField", problemDetails.Extensions.Keys);
        Assert.Equal("customValue", problemDetails.Extensions["customField"]);
    }

    [Fact]
    public void CreateBadRequest_WithExtensionsAndTraceId_CombinesBoth()
    {
        // Arrange
        var extensions = new Dictionary<string, object>
        {
            ["customField"] = "customValue"
        };

        // Act
        var problemDetails = ProblemDetailsFactory.CreateBadRequest("Bad request error", extensions: extensions, traceId: "trace-123");

        // Assert
        Assert.NotNull(problemDetails.Extensions);
        Assert.Contains("customField", problemDetails.Extensions.Keys);
        Assert.Contains("traceId", problemDetails.Extensions.Keys);
        Assert.Equal("customValue", problemDetails.Extensions["customField"]);
        Assert.Equal("trace-123", problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public void CreateNotFound_WithoutTraceId_DoesNotAddExtensions()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateNotFound("Resource not found");

        // Assert
        Assert.True(problemDetails.Extensions == null || problemDetails.Extensions.Count == 0);
    }

    [Fact]
    public void CreateNotFound_WithTraceId_InitializesExtensionsWithTraceId()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateNotFound("Resource not found", traceId: "trace-456");

        // Assert
        Assert.NotNull(problemDetails.Extensions);
        Assert.Contains("traceId", problemDetails.Extensions.Keys);
        Assert.Equal("trace-456", problemDetails.Extensions["traceId"]);
    }

    [Fact]
    public void CreateForbidden_WithoutTraceId_DoesNotAddExtensions()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateForbidden("Access denied");

        // Assert
        Assert.True(problemDetails.Extensions == null || problemDetails.Extensions.Count == 0);
    }

    [Fact]
    public void CreateConflict_WithoutTraceId_DoesNotAddExtensions()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateConflict("Resource conflict");

        // Assert
        Assert.True(problemDetails.Extensions == null || problemDetails.Extensions.Count == 0);
    }

    [Fact]
    public void CreateUnauthorized_WithoutTraceId_DoesNotAddExtensions()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateUnauthorized("Unauthorized access");

        // Assert
        Assert.True(problemDetails.Extensions == null || problemDetails.Extensions.Count == 0);
    }

    [Fact]
    public void CreateInternalServerError_WithoutExtensionsOrTraceId_DoesNotAddExtensions()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateInternalServerError("Internal server error");

        // Assert
        Assert.True(problemDetails.Extensions == null || problemDetails.Extensions.Count == 0);
    }

    [Fact]
    public void CreateInternalServerError_WithExtensions_InitializesExtensionsWithProvidedData()
    {
        // Arrange
        var extensions = new Dictionary<string, object>
        {
            ["stackTrace"] = "stack trace here",
            ["exceptionType"] = "InvalidOperationException"
        };

        // Act
        var problemDetails = ProblemDetailsFactory.CreateInternalServerError("Internal server error", extensions: extensions);

        // Assert
        Assert.NotNull(problemDetails.Extensions);
        Assert.Contains("stackTrace", problemDetails.Extensions.Keys);
        Assert.Contains("exceptionType", problemDetails.Extensions.Keys);
    }

    [Fact]
    public void CreateBadRequest_SetsCorrectStatusAndType()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateBadRequest("Bad request error");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1", problemDetails.Type);
        Assert.Equal("Bad Request", problemDetails.Title);
        Assert.Equal("Bad request error", problemDetails.Detail);
    }

    [Fact]
    public void CreateNotFound_SetsCorrectStatusAndType()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateNotFound("Not found");

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4", problemDetails.Type);
        Assert.Equal("Not Found", problemDetails.Title);
    }

    [Fact]
    public void CreateForbidden_SetsCorrectStatusAndType()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateForbidden("Access denied");

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3", problemDetails.Type);
        Assert.Equal("Forbidden", problemDetails.Title);
    }

    [Fact]
    public void CreateConflict_SetsCorrectStatusAndType()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateConflict("Conflict detected");

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8", problemDetails.Type);
        Assert.Equal("Conflict", problemDetails.Title);
    }

    [Fact]
    public void CreateUnauthorized_SetsCorrectStatusAndType()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateUnauthorized("Unauthorized");

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7235#section-3.1", problemDetails.Type);
        Assert.Equal("Unauthorized", problemDetails.Title);
    }

    [Fact]
    public void CreateInternalServerError_SetsCorrectStatusAndType()
    {
        // Act
        var problemDetails = ProblemDetailsFactory.CreateInternalServerError("Server error");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1", problemDetails.Type);
        Assert.Equal("An error occurred while processing your request", problemDetails.Title);
    }

    [Fact]
    public void GetTraceId_WithCurrentActivity_ReturnsActivityId()
    {
        // Arrange
        var activity = new System.Diagnostics.Activity("test");
        activity.Start();

        // Act
        var traceId = ProblemDetailsFactory.GetTraceId();

        // Assert
        Assert.NotNull(traceId);
        Assert.Equal(activity.Id, traceId);

        activity.Stop();
    }

    [Fact]
    public void GetTraceId_WithHttpContext_ReturnsTraceIdentifier()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "http-trace-123";

        // Act
        var traceId = ProblemDetailsFactory.GetTraceId(httpContext);

        // Assert
        Assert.Equal("http-trace-123", traceId);
    }
}
