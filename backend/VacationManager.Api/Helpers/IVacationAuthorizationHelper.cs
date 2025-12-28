using System.Security.Claims;
using VacationManager.Core.Authorization;
using VacationManager.Core.Entities;

namespace VacationManager.Api.Helpers;

/// <summary>
/// Interface for vacation authorization helper
/// </summary>
public interface IVacationAuthorizationHelper
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
}
