using System.Security.Claims;
using VacationManager.Core.Authorization;
using VacationManager.Core.Entities;

namespace VacationManager.Api.Helpers;

/// <summary>
/// Interface for authorization helper
/// </summary>
public interface IAuthorizationHelper
{
    Task<(User? user, AuthorizationResult result)> AuthorizeUserAsync(ClaimsPrincipal claimsPrincipal);
    Task<(User? user, AuthorizationResult result)> AuthorizeTeamOperationAsync(ClaimsPrincipal claimsPrincipal);
    Task<(User? user, AuthorizationResult result)> AuthorizeManagerOperationAsync(ClaimsPrincipal claimsPrincipal);
    Task<(User? user, AuthorizationResult authResult)> AuthorizeVacationOwnershipAsync(
        ClaimsPrincipal claimsPrincipal,
        Vacation vacation,
        Guid? vacationOwnerTeamId = null);
    Task<(User? user, AuthorizationResult authResult)> AuthorizeApprovalAsync(
        ClaimsPrincipal claimsPrincipal,
        Guid targetUserTeamId);
    
    Task<(User? user, AuthorizationResult result)> EnsureManagerAsync(ClaimsPrincipal claimsPrincipal);
    Task<(User? user, AuthorizationResult result)> EnsureAuthenticatedAsync(ClaimsPrincipal claimsPrincipal);
    Task<(User? user, AuthorizationResult result)> EnsureTeamMemberOrManagerAsync(ClaimsPrincipal claimsPrincipal, Guid teamId);
}
