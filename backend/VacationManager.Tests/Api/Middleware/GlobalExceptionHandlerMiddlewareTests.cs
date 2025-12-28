using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using VacationManager.Api.Middleware;

namespace VacationManager.Tests.Api.Middleware;

public class GlobalExceptionHandlerMiddlewareTests : IDisposable
{
    private TestServer? _server;
    private HttpClient? _client;

    private void SetupServer(string environmentName, RequestDelegate requestDelegate)
    {
        var builder = new WebHostBuilder()
            .UseEnvironment(environmentName)
            .ConfigureServices(services =>
            {
                services.AddLogging();
            })
            .Configure(app =>
            {
                app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
                app.Run(requestDelegate);
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }

    [Fact]
    public async Task GlobalExceptionHandler_WithArgumentException_ReturnsBadRequest()
    {
        // Arrange
        SetupServer(Environments.Development, context =>
        {
            throw new ArgumentException("Invalid argument provided");
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("json", response.Content.Headers.ContentType!.MediaType!);

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails!.Status);
        Assert.Equal("Invalid argument provided", problemDetails.Detail);
        Assert.Equal("Bad Request", problemDetails.Title);
    }

    [Fact]
    public async Task GlobalExceptionHandler_WithUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        SetupServer(Environments.Development, context =>
        {
            throw new UnauthorizedAccessException();
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problemDetails);
        Assert.Equal(401, problemDetails!.Status);
        Assert.Equal("Unauthorized", problemDetails.Title);
        Assert.Contains("not authorized", problemDetails.Detail);
    }

    [Fact]
    public async Task GlobalExceptionHandler_WithKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        SetupServer(Environments.Development, context =>
        {
            throw new KeyNotFoundException("Resource not found");
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problemDetails);
        Assert.Equal(404, problemDetails!.Status);
        Assert.Equal("Resource not found", problemDetails.Detail);
    }

    [Fact]
    public async Task GlobalExceptionHandler_WithGenericException_InDevelopment_ReturnsInternalServerErrorWithDetails()
    {
        // Arrange
        SetupServer(Environments.Development, context =>
        {
            throw new InvalidOperationException("Something went wrong");
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Verify the response contains expected error details
        Assert.Contains("Something went wrong", content);
        Assert.Contains("500", content);
        
        // In development, should include stack trace and exception type information
        Assert.Contains("stackTrace", content);
        Assert.Contains("exceptionType", content);
        Assert.Contains("InvalidOperationException", content);
    }

    [Fact]
    public async Task GlobalExceptionHandler_WithGenericException_InProduction_ReturnsInternalServerErrorWithoutDetails()
    {
        // Arrange
        SetupServer(Environments.Production, context =>
        {
            throw new InvalidOperationException("Sensitive error details");
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();

        // Should contain generic error message
        Assert.Contains("An unexpected error occurred", content);
        Assert.Contains("500", content);
        
        // In production, should not include sensitive data
        Assert.DoesNotContain("Sensitive error details", content);
        Assert.DoesNotContain("stackTrace", content);
        Assert.DoesNotContain("exceptionType", content);
    }

    [Fact]
    public async Task GlobalExceptionHandler_IncludesTraceId()
    {
        // Arrange
        SetupServer(Environments.Development, context =>
        {
            throw new ArgumentException("Test exception");
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        
        // Should include trace ID in the response
        Assert.Contains("traceId", content);
    }

    [Fact]
    public async Task GlobalExceptionHandler_IncludesRequestPath()
    {
        // Arrange
        SetupServer(Environments.Development, context =>
        {
            throw new ArgumentException("Test exception");
        });

        // Act
        var response = await _client!.GetAsync("/api/test/resource");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problemDetails);
        Assert.Equal("/api/test/resource", problemDetails!.Instance);
    }

    [Fact]
    public async Task GlobalExceptionHandler_WithNoException_AllowsRequestToProceed()
    {
        // Arrange
        SetupServer(Environments.Development, async context =>
        {
            await context.Response.WriteAsync("Success");
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Success", content);
    }

    [Fact]
    public async Task GlobalExceptionHandler_ReturnsCorrectContentType()
    {
        // Arrange
        SetupServer(Environments.Development, context =>
        {
            throw new ArgumentException("Test exception");
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("json", response.Content.Headers.ContentType!.MediaType!);
    }

    [Fact]
    public async Task GlobalExceptionHandler_UsesCamelCaseJsonSerialization()
    {
        // Arrange
        SetupServer(Environments.Development, context =>
        {
            throw new ArgumentException("Test exception");
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        
        // Should use camelCase
        Assert.Contains("\"status\"", content);
        Assert.Contains("\"title\"", content);
        Assert.Contains("\"detail\"", content);
        
        // Should not use PascalCase
        Assert.DoesNotContain("\"Status\"", content);
        Assert.DoesNotContain("\"Title\"", content);
    }

    [Fact]
    public async Task GlobalExceptionHandler_WithDifferentExceptionTypes_ReturnsDifferentStatusCodes()
    {
        // ArgumentException -> 400
        SetupServer(Environments.Development, context => throw new ArgumentException("Bad request"));
        var response1 = await _client!.GetAsync("/test");
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response1.StatusCode);

        // UnauthorizedAccessException -> 401
        SetupServer(Environments.Development, context => throw new UnauthorizedAccessException());
        _client = _server!.CreateClient();
        var response2 = await _client.GetAsync("/test");
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response2.StatusCode);

        // KeyNotFoundException -> 404
        SetupServer(Environments.Development, context => throw new KeyNotFoundException("Not found"));
        _client = _server!.CreateClient();
        var response3 = await _client.GetAsync("/test");
        Assert.Equal(StatusCodes.Status404NotFound, (int)response3.StatusCode);

        // Other exceptions -> 500
        SetupServer(Environments.Development, context => throw new InvalidOperationException("Server error"));
        _client = _server!.CreateClient();
        var response4 = await _client.GetAsync("/test");
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response4.StatusCode);
    }

    [Fact]
    public async Task GlobalExceptionHandler_PreservesRFC7807ProblemDetailsStructure()
    {
        // Arrange
        SetupServer(Environments.Development, context =>
        {
            throw new ArgumentException("Test exception");
        });

        // Act
        var response = await _client!.GetAsync("/test");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // RFC 7807 required fields
        Assert.True(problemDetails.TryGetProperty("type", out _));
        Assert.True(problemDetails.TryGetProperty("title", out _));
        Assert.True(problemDetails.TryGetProperty("status", out _));
        Assert.True(problemDetails.TryGetProperty("detail", out _));
        Assert.True(problemDetails.TryGetProperty("instance", out _));
    }

    public void Dispose()
    {
        _client?.Dispose();
        _server?.Dispose();
    }
}
