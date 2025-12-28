using Microsoft.AspNetCore.Http;
using VacationManager.Api.Helpers;
using Xunit;

namespace VacationManager.Tests.Api.Helpers;

public class ProblemDetailsBuilderTests
{
    [Fact]
    public void BadRequest_WithDetail_CreatesProblemDetails()
    {
        // Arrange & Act
        var result = ProblemDetailsBuilder.BadRequest()
            .WithDetail("Invalid request data")
            .Build();

        // Assert
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1", result.Type);
        Assert.Equal("Bad Request", result.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Status);
        Assert.Equal("Invalid request data", result.Detail);
    }

    [Fact]
    public void NotFound_WithCustomTitle_OverridesDefaultTitle()
    {
        // Arrange & Act
        var result = ProblemDetailsBuilder.NotFound()
            .WithTitle("Resource Not Found")
            .WithDetail("User not found")
            .Build();

        // Assert
        Assert.Equal("Resource Not Found", result.Title);
        Assert.Equal("User not found", result.Detail);
        Assert.Equal(StatusCodes.Status404NotFound, result.Status);
    }

    [Fact]
    public void WithExtension_AddsCustomExtension()
    {
        // Arrange & Act
        var result = ProblemDetailsBuilder.BadRequest()
            .WithDetail("Validation failed")
            .WithExtension("errorCode", "VALIDATION_ERROR")
            .WithExtension("field", "email")
            .Build();

        // Assert
        Assert.NotNull(result.Extensions);
        Assert.Equal("VALIDATION_ERROR", result.Extensions["errorCode"]);
        Assert.Equal("email", result.Extensions["field"]);
    }

    [Fact]
    public void WithTraceId_AddsTraceIdExtension()
    {
        // Arrange
        var traceId = "test-trace-123";

        // Act
        var result = ProblemDetailsBuilder.InternalServerError()
            .WithDetail("Server error")
            .WithTraceId(traceId)
            .Build();

        // Assert
        Assert.NotNull(result.Extensions);
        Assert.Equal(traceId, result.Extensions["traceId"]);
    }

    [Fact]
    public void WithValidationErrors_AddsErrorsDictionary()
    {
        // Arrange
        var errors = new Dictionary<string, object>
        {
            ["email"] = new[] { "Email is required", "Invalid email format" },
            ["password"] = new[] { "Password must be at least 8 characters" }
        };

        // Act
        var result = ProblemDetailsBuilder.BadRequest()
            .WithDetail("Validation failed")
            .WithValidationErrors(errors)
            .Build();

        // Assert
        Assert.NotNull(result.Extensions);
        Assert.Equal(errors, result.Extensions["errors"]);
    }

    [Fact]
    public void Forbidden_WithInstance_SetInstanceProperty()
    {
        // Arrange & Act
        var result = ProblemDetailsBuilder.Forbidden()
            .WithDetail("Access denied")
            .WithInstance("/api/vacations/123")
            .Build();

        // Assert
        Assert.Equal("/api/vacations/123", result.Instance);
        Assert.Equal(StatusCodes.Status403Forbidden, result.Status);
    }

    [Fact]
    public void Conflict_CreatesCorrectProblemDetails()
    {
        // Arrange & Act
        var result = ProblemDetailsBuilder.Conflict()
            .WithDetail("Resource already exists")
            .Build();

        // Assert
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8", result.Type);
        Assert.Equal("Conflict", result.Title);
        Assert.Equal(StatusCodes.Status409Conflict, result.Status);
    }

    [Fact]
    public void Unauthorized_CreatesCorrectProblemDetails()
    {
        // Arrange & Act
        var result = ProblemDetailsBuilder.Unauthorized()
            .WithDetail("Invalid credentials")
            .Build();

        // Assert
        Assert.Equal("https://datatracker.ietf.org/doc/html/rfc7235#section-3.1", result.Type);
        Assert.Equal("Unauthorized", result.Title);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.Status);
    }

    [Fact]
    public void FluentInterface_AllowsChaining()
    {
        // Arrange & Act
        var result = ProblemDetailsBuilder.BadRequest()
            .WithTitle("Custom Title")
            .WithDetail("Custom detail message")
            .WithInstance("/api/test")
            .WithExtension("code", "ERR001")
            .WithTraceId("trace-123")
            .Build();

        // Assert
        Assert.Equal("Custom Title", result.Title);
        Assert.Equal("Custom detail message", result.Detail);
        Assert.Equal("/api/test", result.Instance);
        Assert.NotNull(result.Extensions);
        Assert.Equal("ERR001", result.Extensions["code"]);
        Assert.Equal("trace-123", result.Extensions["traceId"]);
    }

    [Fact]
    public void New_CreatesEmptyBuilder()
    {
        // Arrange & Act
        var result = ProblemDetailsBuilder.New()
            .WithType("https://example.com/custom-error")
            .WithTitle("Custom Error")
            .WithStatus(418) // I'm a teapot
            .WithDetail("Custom error message")
            .Build();

        // Assert
        Assert.Equal("https://example.com/custom-error", result.Type);
        Assert.Equal("Custom Error", result.Title);
        Assert.Equal(418, result.Status);
        Assert.Equal("Custom error message", result.Detail);
    }
}
