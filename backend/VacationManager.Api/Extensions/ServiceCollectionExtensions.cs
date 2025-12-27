using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using VacationManager.Data;
using VacationManager.Core.Interfaces;
using VacationManager.Core.Validators;
using VacationManager.Data.Repositories;
using VacationManager.Api.Services;

namespace VacationManager.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("EntraId"));
        
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IVacationRepository, VacationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IClaimExtractorService, ClaimExtractorService>();
        
        return services;
    }

    public static IServiceCollection AddApplicationValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateVacationDtoValidator>();
        
        return services;
    }

    public static IServiceCollection AddApplicationDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        services.AddDbContext<VacationManagerDbContext>(options =>
            options.UseSqlite(connectionString));
        
        return services;
    }

    public static IServiceCollection AddApplicationCors(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        if (allowedOrigins is null || allowedOrigins.Length == 0)
        {
            allowedOrigins = environment.IsProduction()
                ? Array.Empty<string>()
                : new[] { "http://localhost:4200" };
        }

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddApplicationSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var apiClientId = configuration["EntraId:ClientId"];
            var scope = $"api://{apiClientId}/access_as_user";
            
            options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
                Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows
                {
                    Implicit = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{configuration["EntraId:Instance"]}{configuration["EntraId:TenantId"]}/oauth2/v2.0/authorize"),
                        Scopes = new Dictionary<string, string>
                        {
                            { scope, "Access the API" }
                        }
                    }
                }
            });
            
            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { scope }
                }
            });
        });
        
        return services;
    }
}
