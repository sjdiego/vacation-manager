using Microsoft.EntityFrameworkCore;
using VacationManager.Data;

namespace VacationManager.Api.Extensions;

public static class WebApplicationExtensions
{
    public static void UseApplicationSwaggerUI(this WebApplication app, IConfiguration configuration)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.OAuthClientId(configuration["EntraId:SwaggerClientId"]);
                options.OAuthAppName("VacationManager API");
                options.OAuthUsePkce();
            });
        }
    }

    public static void RunDatabaseMigrations(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<VacationManagerDbContext>();
            dbContext.Database.Migrate();
        }
    }
}
