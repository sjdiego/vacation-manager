namespace VacationManager.Core.Authorization;

/// <summary>
/// Service for executing authorization checks
/// </summary>
public class AuthorizationService
{
    /// <summary>
    /// Executes an authorization chain
    /// </summary>
    public async Task<AuthorizationResult> AuthorizeAsync(
        AuthorizationHandler chain,
        AuthorizationContext context)
    {
        return await chain.HandleAsync(context);
    }
}
