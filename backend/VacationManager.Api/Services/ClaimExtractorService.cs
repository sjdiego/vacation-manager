using System.Security.Claims;

namespace VacationManager.Api.Services;

public interface IClaimExtractorService
{
    string GetEntraId(ClaimsPrincipal user);
    string GetEmail(ClaimsPrincipal user);
    string GetName(ClaimsPrincipal user);
}

public class ClaimExtractorService : IClaimExtractorService
{
    private readonly IConfiguration _configuration;

    public ClaimExtractorService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetEntraId(ClaimsPrincipal user)
    {
        var claimType = _configuration["EntraId:Claims:ObjectId"] ?? "http://schemas.microsoft.com/identity/claims/objectidentifier";
        return user.FindFirst(claimType)?.Value ?? "";
    }

    public string GetEmail(ClaimsPrincipal user)
    {
        var claimType = _configuration["EntraId:Claims:Email"] ?? "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        return user.FindFirst(claimType)?.Value ?? "";
    }

    public string GetName(ClaimsPrincipal user)
    {
        var claimType = _configuration["EntraId:Claims:Name"] ?? "name";
        return user.FindFirst(claimType)?.Value ?? "";
    }
}
