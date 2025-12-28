namespace VacationManager.Core.Authorization;

/// <summary>
/// Abstract base class for authorization handlers in a chain of responsibility
/// </summary>
public abstract class AuthorizationHandler
{
    private AuthorizationHandler? _next;

    /// <summary>
    /// Sets the next handler in the chain
    /// </summary>
    public AuthorizationHandler SetNext(AuthorizationHandler handler)
    {
        _next = handler;
        return handler;
    }

    /// <summary>
    /// Handles the authorization check, potentially passing to the next handler
    /// </summary>
    public async Task<AuthorizationResult> HandleAsync(AuthorizationContext context)
    {
        var result = await CheckAuthorizationAsync(context);

        // If this handler denies, stop the chain
        if (!result.IsAuthorized)
        {
            return result;
        }

        // If authorized and there's a next handler, continue the chain
        if (_next != null)
        {
            return await _next.HandleAsync(context);
        }

        // End of chain and authorized
        return result;
    }

    /// <summary>
    /// Implemented by derived classes to perform specific authorization logic
    /// </summary>
    protected abstract Task<AuthorizationResult> CheckAuthorizationAsync(AuthorizationContext context);
}
