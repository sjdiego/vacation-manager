using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VacationManager.Api.Configuration;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly IConfiguration _configuration;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration)
    {
        _provider = provider;
        _configuration = configuration;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var apiClientId = _configuration["EntraId:ClientId"];
        var scope = $"api://{apiClientId}/access_as_user";
        
        // API Versioning
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Vacation Manager API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated ? "This API version has been deprecated." : "Vacation Manager REST API"
            });
        }
        
        // OAuth2 configuration
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                Implicit = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{_configuration["EntraId:Instance"]}{_configuration["EntraId:TenantId"]}/oauth2/v2.0/authorize"),
                    Scopes = new Dictionary<string, string>
                    {
                        { scope, "Access the API" }
                    }
                }
            }
        });
        
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "oauth2"
                    }
                },
                new[] { scope }
            }
        });
        
        // Document Problem Details responses
        options.MapType<ProblemDetails>(() => new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new OpenApiSchema { Type = "string" },
                ["title"] = new OpenApiSchema { Type = "string" },
                ["status"] = new OpenApiSchema { Type = "integer" },
                ["detail"] = new OpenApiSchema { Type = "string" },
                ["instance"] = new OpenApiSchema { Type = "string" },
                ["traceId"] = new OpenApiSchema { Type = "string", Description = "Request trace identifier" },
                ["errors"] = new OpenApiSchema 
                { 
                    Type = "object",
                    Description = "Validation errors dictionary",
                    AdditionalPropertiesAllowed = true,
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "string" }
                    }
                }
            },
            AdditionalPropertiesAllowed = true
        });
    }
}
