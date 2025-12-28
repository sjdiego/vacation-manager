namespace VacationManager.Api.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent clickjacking attacks
        context.Response.Headers["X-Frame-Options"] = "DENY";
        
        // Prevent MIME sniffing (content type detection)
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        
        // Enable HSTS (HTTP Strict Transport Security) - forces HTTPS
        // max-age: 31536000 seconds = 1 year
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        
        // Content Security Policy - restrict resource loading
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";
        
        // Referrer Policy - control referrer information
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        
        // Feature Policy / Permissions Policy - restrict browser APIs
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        await _next(context);
    }
}

