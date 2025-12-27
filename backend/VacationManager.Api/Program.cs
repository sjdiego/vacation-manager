using Serilog;
using AspNetCoreRateLimit;
using Asp.Versioning;
using VacationManager.Api.Extensions;
using VacationManager.Api.Middleware;
using VacationManager.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

// Logging with Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc();

// Add services using extension methods
builder.Services.AddApplicationAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddApplicationValidation();
builder.Services.AddApplicationDatabase(builder.Configuration);
builder.Services.AddApplicationRateLimiting(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddApplicationCors(builder.Configuration, builder.Environment);
builder.Services.AddApplicationSwagger(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseApplicationSwaggerUI(app.Configuration);
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Database migration
app.RunDatabaseMigrations();

app.Run();
