using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using VacationManager.Data;
using VacationManager.Core.Interfaces;
using VacationManager.Core.Validators;
using VacationManager.Core.Validation;
using VacationManager.Core.Validation.Rules;
using VacationManager.Data.Repositories;
using VacationManager.Api.Services;
using VacationManager.Api.Helpers;
using VacationManager.Api.Configuration;

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
        
        // Register validation rules
        services.AddScoped<IVacationValidationRule, TeamMembershipValidationRule>();
        services.AddScoped<IVacationValidationRule, VacationOverlapValidationRule>();
        
        // Register validation service
        services.AddScoped<IVacationValidationService, VacationValidationService>();
        
        // Register authorization service
        services.AddScoped<VacationManager.Core.Authorization.AuthorizationService>();
        services.AddScoped<IVacationAuthorizationHelper, VacationAuthorizationHelper>();
        
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

    public static IServiceCollection AddApplicationVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

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
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();
        
        return services;
    }
}
