using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using VacationManager.Api.Middleware;

namespace VacationManager.Tests.Api.Middleware;

public class SecurityHeadersMiddlewareTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;

    public SecurityHeadersMiddlewareTests()
    {
        var builder = new WebHostBuilder()
            .Configure(app =>
            {
                app.UseMiddleware<SecurityHeadersMiddleware>();
                app.Run(async context => await context.Response.WriteAsync("OK"));
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_AddsXFrameOptionsHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_AddsXContentTypeOptionsHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_AddsStrictTransportSecurityHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("Strict-Transport-Security"));
        var hstsValue = response.Headers.GetValues("Strict-Transport-Security").First();
        Assert.Contains("max-age=31536000", hstsValue);
        Assert.Contains("includeSubDomains", hstsValue);
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_AddsCspHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        var cspValue = response.Headers.GetValues("Content-Security-Policy").First();
        Assert.Contains("default-src 'self'", cspValue);
        Assert.Contains("script-src 'self'", cspValue);
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_AddsReferrerPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").First());
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_AddsPermissionsPolicyHeader()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("Permissions-Policy"));
        var ppValue = response.Headers.GetValues("Permissions-Policy").First();
        Assert.Contains("geolocation=()", ppValue);
        Assert.Contains("microphone=()", ppValue);
        Assert.Contains("camera=()", ppValue);
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_AllHeadersPresent()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        var requiredHeaders = new[]
        {
            "X-Frame-Options",
            "X-Content-Type-Options",
            "Strict-Transport-Security",
            "Content-Security-Policy",
            "Referrer-Policy",
            "Permissions-Policy"
        };

        foreach (var header in requiredHeaders)
        {
            Assert.True(response.Headers.Contains(header), $"Missing security header: {header}");
        }
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_RequestIsProcessed()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("OK", content);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _server?.Dispose();
    }
}
