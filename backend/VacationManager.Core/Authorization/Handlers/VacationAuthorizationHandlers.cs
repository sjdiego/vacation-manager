using VacationManager.Core.Entities;

namespace VacationManager.Core.Authorization.Handlers;

/// <summary>
/// Ensures the user exists in the system
/// </summary>
public class UserExistsHandler : AuthorizationHandler
{
    protected override Task<AuthorizationResult> CheckAuthorizationAsync(AuthorizationContext context)
    {
        if (context.User == null)
        {
            return Task.FromResult(
                AuthorizationResult.Failure("User not found", "USER_NOT_FOUND"));
        }

        return Task.FromResult(AuthorizationResult.Success());
    }
}

/// <summary>
/// Ensures the user is a member of a team
/// </summary>
public class TeamMembershipHandler : AuthorizationHandler
{
    protected override Task<AuthorizationResult> CheckAuthorizationAsync(AuthorizationContext context)
    {
        if (context.User.TeamId == null)
        {
            return Task.FromResult(
                AuthorizationResult.Failure(
                    "User must be part of a team",
                    "TEAM_MEMBERSHIP_REQUIRED"));
        }

        return Task.FromResult(AuthorizationResult.Success());
    }
}

/// <summary>
/// Ensures the user has manager role
/// </summary>
public class ManagerRoleHandler : AuthorizationHandler
{
    protected override Task<AuthorizationResult> CheckAuthorizationAsync(AuthorizationContext context)
    {
        if (!context.User.IsManager)
        {
            return Task.FromResult(
                AuthorizationResult.Failure(
                    "Only managers can perform this operation",
                    "MANAGER_ROLE_REQUIRED"));
        }

        return Task.FromResult(AuthorizationResult.Success());
    }
}

/// <summary>
/// Ensures the user is the owner of the vacation or a manager in the same team
/// </summary>
public class VacationOwnershipHandler : AuthorizationHandler
{
    protected override Task<AuthorizationResult> CheckAuthorizationAsync(AuthorizationContext context)
    {
        if (context.Resource is not Vacation vacation)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }

        var isOwner = vacation.UserId == context.User.Id;
        
        // Check if it's a manager from the same team
        var isManagerInSameTeam = context.User.IsManager &&
                                  context.AdditionalData.TryGetValue("VacationOwnerTeamId", out var teamIdObj) &&
                                  teamIdObj is Guid teamId &&
                                  context.User.TeamId == teamId;

        if (!isOwner && !isManagerInSameTeam)
        {
            return Task.FromResult(
                AuthorizationResult.Failure(
                    "You don't have permission to access this vacation",
                    "OWNERSHIP_REQUIRED"));
        }

        return Task.FromResult(AuthorizationResult.Success());
    }
}

/// <summary>
/// Ensures the manager can only manage vacations for their team members
/// </summary>
public class SameTeamHandler : AuthorizationHandler
{
    protected override Task<AuthorizationResult> CheckAuthorizationAsync(AuthorizationContext context)
    {
        if (!context.AdditionalData.TryGetValue("TargetUserTeamId", out var targetTeamIdObj) ||
            targetTeamIdObj is not Guid targetTeamId)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }

        if (context.User.TeamId != targetTeamId)
        {
            return Task.FromResult(
                AuthorizationResult.Failure(
                    "You can only manage vacations for your team members",
                    "SAME_TEAM_REQUIRED"));
        }

        return Task.FromResult(AuthorizationResult.Success());
    }
}
