using VacationManager.Api.Services;
using VacationManager.Core.Authorization;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VacationManager.Api.Helpers;

/// <summary>
/// Helper service to simplify authorization checks in controllers
/// </summary>
public class AuthorizationHelper : IAuthorizationHelper
{
    private readonly IUserRepository _userRepository;
    private readonly AuthorizationService _authorizationService;
    private readonly IClaimExtractorService _claimExtractor;

    public AuthorizationHelper(
        IUserRepository userRepository,
        AuthorizationService authorizationService,
        IClaimExtractorService claimExtractor)
    {
        _userRepository = userRepository;
        _authorizationService = authorizationService;
        _claimExtractor = claimExtractor;
    }

    /// <summary>
    /// Gets the current user without authorization checks
    /// </summary>
    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        var userEntraId = _claimExtractor.GetEntraId(claimsPrincipal);
        if (string.IsNullOrEmpty(userEntraId))
            return null;

        return await _userRepository.GetByEntraIdAsync(userEntraId);
    }

    /// <summary>
    /// Gets the current user from claims and checks authorization
    /// </summary>
    public async Task<(User? user, AuthorizationResult authResult)> AuthorizeAsync(
        ClaimsPrincipal claimsPrincipal,
        AuthorizationHandler chain,
        string operation,
        object? resource = null,
        Dictionary<string, object>? additionalData = null)
    {
        var userEntraId = _claimExtractor.GetEntraId(claimsPrincipal);
        if (string.IsNullOrEmpty(userEntraId))
        {
            return (null, AuthorizationResult.Failure("Unauthorized", "UNAUTHORIZED"));
        }

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null)
        {
            return (null, AuthorizationResult.Failure("User not found", "USER_NOT_FOUND"));
        }

        var context = new AuthorizationContext
        {
            User = user,
            Operation = operation,
            Resource = resource,
            AdditionalData = additionalData ?? new Dictionary<string, object>()
        };

        var authResult = await _authorizationService.AuthorizeAsync(chain, context);
        return (user, authResult);
    }

    /// <summary>
    /// Simplified authorization for basic user operations (just checks user exists)
    /// </summary>
    public async Task<(User? user, AuthorizationResult result)> AuthorizeUserAsync(
        ClaimsPrincipal claimsPrincipal)
    {
        return await AuthorizeAsync(
            claimsPrincipal,
            AuthorizationChainFactory.CreateVacationChain(),
            "UserOperation");
    }

    /// <summary>
    /// Simplified authorization for team operations
    /// </summary>
    public async Task<(User? user, AuthorizationResult result)> AuthorizeTeamOperationAsync(
        ClaimsPrincipal claimsPrincipal)
    {
        return await AuthorizeAsync(
            claimsPrincipal,
            AuthorizationChainFactory.CreateViewTeamVacationsChain(),
            "TeamOperation");
    }

    /// <summary>
    /// Simplified authorization for manager operations
    /// </summary>
    public async Task<(User? user, AuthorizationResult result)> AuthorizeManagerOperationAsync(
        ClaimsPrincipal claimsPrincipal)
    {
        return await AuthorizeAsync(
            claimsPrincipal,
            AuthorizationChainFactory.CreateManagerOperationChain(),
            "ManagerOperation");
    }

    /// <summary>
    /// Simplified authorization for vacation ownership
    /// </summary>
    public async Task<(User? user, AuthorizationResult authResult)> AuthorizeVacationOwnershipAsync(
        ClaimsPrincipal claimsPrincipal,
        Vacation vacation,
        Guid? vacationOwnerTeamId = null)
    {
        var additionalData = vacationOwnerTeamId.HasValue
            ? new Dictionary<string, object> { ["VacationOwnerTeamId"] = vacationOwnerTeamId.Value }
            : null;

        return await AuthorizeAsync(
            claimsPrincipal,
            AuthorizationChainFactory.CreateVacationOwnershipChain(),
            "VacationOwnership",
            vacation,
            additionalData);
    }

    /// <summary>
    /// Simplified authorization for approving vacations
    /// </summary>
    public async Task<(User? user, AuthorizationResult authResult)> AuthorizeApprovalAsync(
        ClaimsPrincipal claimsPrincipal,
        Guid targetUserTeamId)
    {
        return await AuthorizeAsync(
            claimsPrincipal,
            AuthorizationChainFactory.CreateApproveVacationChain(),
            "ApproveVacation",
            null,
            new Dictionary<string, object> { ["TargetUserTeamId"] = targetUserTeamId });
    }

    /// <summary>
    /// Ensures user is a manager and returns authorization result
    /// </summary>
    public async Task<(User? user, AuthorizationResult result)> EnsureManagerAsync(ClaimsPrincipal claimsPrincipal)
    {
        return await AuthorizeManagerOperationAsync(claimsPrincipal);
    }

    /// <summary>
    /// Ensures user is authenticated and returns authorization result
    /// </summary>
    public async Task<(User? user, AuthorizationResult result)> EnsureAuthenticatedAsync(ClaimsPrincipal claimsPrincipal)
    {
        return await AuthorizeUserAsync(claimsPrincipal);
    }

    /// <summary>
    /// Ensures user is a member of the specified team or a manager
    /// </summary>
    public async Task<(User? user, AuthorizationResult result)> EnsureTeamMemberOrManagerAsync(ClaimsPrincipal claimsPrincipal, Guid teamId)
    {
        var (user, result) = await EnsureAuthenticatedAsync(claimsPrincipal);
        
        if (!result.IsAuthorized || user == null)
            return (user, result);
        
        if (user.IsManager || user.TeamId == teamId)
            return (user, AuthorizationResult.Success());
        
        return (user, AuthorizationResult.Failure("User is not a member of this team and is not a manager"));
    }
}
