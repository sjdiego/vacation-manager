using AspNetCoreRateLimit;

namespace VacationManager.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApplicationRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimitOptions"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        
        return services;
    }
}


