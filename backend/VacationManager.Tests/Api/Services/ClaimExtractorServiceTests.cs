using Xunit;
using NSubstitute;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using VacationManager.Api.Services;

namespace VacationManager.Tests.Api.Services;

public class ClaimExtractorServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly ClaimExtractorService _service;

    public ClaimExtractorServiceTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _configuration["EntraId:Claims:ObjectId"].Returns("http://schemas.microsoft.com/identity/claims/objectidentifier");
        _configuration["EntraId:Claims:Email"].Returns("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
        _configuration["EntraId:Claims:Name"].Returns("name");
        _service = new ClaimExtractorService(_configuration);
    }

    [Fact]
    public void GetEntraId_WithValidClaim_ReturnsEntraId()
    {
        // Arrange
        var entraId = "user-entra-id-123";
        var claims = new[] { new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", entraId) };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.GetEntraId(user);

        // Assert
        Assert.Equal(entraId, result);
    }

    [Fact]
    public void GetEntraId_WithMissingClaim_ReturnsEmpty()
    {
        // Arrange
        var claims = new Claim[] { };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.GetEntraId(user);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void GetEntraId_WithCustomConfigClaimType_ReturnsEntraId()
    {
        // Arrange
        _configuration["EntraId:Claims:ObjectId"].Returns("custom:objectid");
        var service = new ClaimExtractorService(_configuration);
        var entraId = "custom-entra-id";
        var claims = new[] { new Claim("custom:objectid", entraId) };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = service.GetEntraId(user);

        // Assert
        Assert.Equal(entraId, result);
    }

    [Fact]
    public void GetEmail_WithValidClaim_ReturnsEmail()
    {
        // Arrange
        var email = "user@example.com";
        var claims = new[] { new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", email) };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.GetEmail(user);

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void GetEmail_WithMissingClaim_ReturnsEmpty()
    {
        // Arrange
        var claims = new Claim[] { };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.GetEmail(user);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void GetEmail_WithCustomConfigClaimType_ReturnsEmail()
    {
        // Arrange
        _configuration["EntraId:Claims:Email"].Returns("custom:email");
        var service = new ClaimExtractorService(_configuration);
        var email = "custom@example.com";
        var claims = new[] { new Claim("custom:email", email) };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = service.GetEmail(user);

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void GetName_WithValidClaim_ReturnsName()
    {
        // Arrange
        var name = "John Doe";
        var claims = new[] { new Claim("name", name) };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.GetName(user);

        // Assert
        Assert.Equal(name, result);
    }

    [Fact]
    public void GetName_WithMissingClaim_ReturnsEmpty()
    {
        // Arrange
        var claims = new Claim[] { };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = _service.GetName(user);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void GetName_WithCustomConfigClaimType_ReturnsName()
    {
        // Arrange
        _configuration["EntraId:Claims:Name"].Returns("custom:name");
        var service = new ClaimExtractorService(_configuration);
        var name = "Jane Smith";
        var claims = new[] { new Claim("custom:name", name) };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var result = service.GetName(user);

        // Assert
        Assert.Equal(name, result);
    }

    [Fact]
    public void GetEntraId_GetEmail_GetName_WithMultipleClaims_ReturnsCorrectValues()
    {
        // Arrange
        var entraId = "entra-123";
        var email = "user@example.com";
        var name = "John Doe";
        var claims = new[]
        {
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", entraId),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", email),
            new Claim("name", name),
            new Claim("other-claim", "other-value")
        };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        // Act
        var resultEntraId = _service.GetEntraId(user);
        var resultEmail = _service.GetEmail(user);
        var resultName = _service.GetName(user);

        // Assert
        Assert.Equal(entraId, resultEntraId);
        Assert.Equal(email, resultEmail);
        Assert.Equal(name, resultName);
    }
}
