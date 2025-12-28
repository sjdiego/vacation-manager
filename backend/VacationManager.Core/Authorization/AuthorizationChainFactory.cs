using VacationManager.Core.Authorization.Handlers;

namespace VacationManager.Core.Authorization;

/// <summary>
/// Factory for creating pre-configured authorization chains
/// </summary>
public static class AuthorizationChainFactory
{
    /// <summary>
    /// Creates a chain for viewing team pending vacations (requires manager in team)
    /// </summary>
    public static AuthorizationHandler CreateViewTeamPendingVacationsChain()
    {
        var userExists = new UserExistsHandler();
        var hasTeam = new TeamMembershipHandler();
        var isManager = new ManagerRoleHandler();

        userExists.SetNext(hasTeam).SetNext(isManager);

        return userExists;
    }

    /// <summary>
    /// Creates a chain for manager operations (only requires manager role, no team membership)
    /// </summary>
    public static AuthorizationHandler CreateManagerOperationChain()
    {
        var userExists = new UserExistsHandler();
        var isManager = new ManagerRoleHandler();

        userExists.SetNext(isManager);

        return userExists;
    }

    /// <summary>
    /// Creates a chain for creating vacations (requires team membership)
    /// </summary>
    public static AuthorizationHandler CreateVacationChain()
    {
        var userExists = new UserExistsHandler();
        var hasTeam = new TeamMembershipHandler();

        userExists.SetNext(hasTeam);

        return userExists;
    }

    /// <summary>
    /// Creates a chain for updating/deleting own vacations (requires ownership)
    /// </summary>
    public static AuthorizationHandler CreateVacationOwnershipChain()
    {
        var userExists = new UserExistsHandler();
        var ownership = new VacationOwnershipHandler();

        userExists.SetNext(ownership);

        return userExists;
    }

    /// <summary>
    /// Creates a chain for approving vacations (requires manager of same team)
    /// </summary>
    public static AuthorizationHandler CreateApproveVacationChain()
    {
        var userExists = new UserExistsHandler();
        var isManager = new ManagerRoleHandler();
        var sameTeam = new SameTeamHandler();

        userExists.SetNext(isManager).SetNext(sameTeam);

        return userExists;
    }

    /// <summary>
    /// Creates a chain for viewing team vacations (requires team membership)
    /// </summary>
    public static AuthorizationHandler CreateViewTeamVacationsChain()
    {
        var userExists = new UserExistsHandler();
        var hasTeam = new TeamMembershipHandler();

        userExists.SetNext(hasTeam);

        return userExists;
    }
}
