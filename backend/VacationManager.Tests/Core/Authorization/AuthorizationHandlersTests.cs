using Xunit;
using VacationManager.Core.Entities;
using VacationManager.Core.Authorization;
using VacationManager.Core.Authorization.Handlers;

namespace VacationManager.Tests.Core.Authorization;

public class AuthorizationHandlersTests
{
    [Fact]
    public async Task UserExistsHandler_WithValidUser_ReturnsSuccess()
    {
        // Arrange
        var handler = new UserExistsHandler();
        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "test@example.com",
                DisplayName = "Test User"
            }
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public async Task UserExistsHandler_WithNullUser_ReturnsFailure()
    {
        // Arrange
        var handler = new UserExistsHandler();
        var context = new AuthorizationContext
        {
            User = null!
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal("User not found", result.FailureReason);
        Assert.Equal("USER_NOT_FOUND", result.FailureCode);
    }

    [Fact]
    public async Task TeamMembershipHandler_WithTeamMember_ReturnsSuccess()
    {
        // Arrange
        var handler = new TeamMembershipHandler();
        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "test@example.com",
                DisplayName = "Test User",
                TeamId = Guid.NewGuid()
            }
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public async Task TeamMembershipHandler_WithoutTeam_ReturnsFailure()
    {
        // Arrange
        var handler = new TeamMembershipHandler();
        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "test@example.com",
                DisplayName = "Test User",
                TeamId = null
            }
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal("User must be part of a team", result.FailureReason);
        Assert.Equal("TEAM_MEMBERSHIP_REQUIRED", result.FailureCode);
    }

    [Fact]
    public async Task ManagerRoleHandler_WithManager_ReturnsSuccess()
    {
        // Arrange
        var handler = new ManagerRoleHandler();
        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "manager@example.com",
                DisplayName = "Manager User",
                IsManager = true
            }
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public async Task ManagerRoleHandler_WithoutManagerRole_ReturnsFailure()
    {
        // Arrange
        var handler = new ManagerRoleHandler();
        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "user@example.com",
                DisplayName = "Regular User",
                IsManager = false
            }
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal("Only managers can perform this operation", result.FailureReason);
        Assert.Equal("MANAGER_ROLE_REQUIRED", result.FailureCode);
    }

    [Fact]
    public async Task VacationOwnershipHandler_WithOwner_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new VacationOwnershipHandler();
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = userId,
                EntraId = "entra-123",
                Email = "user@example.com",
                DisplayName = "User"
            },
            Resource = vacation
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public async Task VacationOwnershipHandler_WithNonOwner_ReturnsFailure()
    {
        // Arrange
        var handler = new VacationOwnershipHandler();
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "user@example.com",
                DisplayName = "User"
            },
            Resource = vacation
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal("You don't have permission to access this vacation", result.FailureReason);
        Assert.Equal("OWNERSHIP_REQUIRED", result.FailureCode);
    }

    [Fact]
    public async Task SameTeamHandler_WithSameTeam_ReturnsSuccess()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var handler = new SameTeamHandler();
        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "manager@example.com",
                DisplayName = "Manager",
                TeamId = teamId
            },
            AdditionalData = new Dictionary<string, object>
            {
                ["TargetUserTeamId"] = teamId
            }
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public async Task SameTeamHandler_WithDifferentTeam_ReturnsFailure()
    {
        // Arrange
        var handler = new SameTeamHandler();
        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "manager@example.com",
                DisplayName = "Manager",
                TeamId = Guid.NewGuid()
            },
            AdditionalData = new Dictionary<string, object>
            {
                ["TargetUserTeamId"] = Guid.NewGuid()
            }
        };

        // Act
        var result = await handler.HandleAsync(context);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal("You can only manage vacations for your team members", result.FailureReason);
        Assert.Equal("SAME_TEAM_REQUIRED", result.FailureCode);
    }

    [Fact]
    public async Task ChainOfHandlers_AllPass_ReturnsSuccess()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var userExists = new UserExistsHandler();
        var hasTeam = new TeamMembershipHandler();
        var isManager = new ManagerRoleHandler();

        userExists.SetNext(hasTeam).SetNext(isManager);

        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "manager@example.com",
                DisplayName = "Manager",
                TeamId = teamId,
                IsManager = true
            }
        };

        // Act
        var result = await userExists.HandleAsync(context);

        // Assert
        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public async Task ChainOfHandlers_MiddleFails_ReturnsFailureAndStopsChain()
    {
        // Arrange
        var userExists = new UserExistsHandler();
        var hasTeam = new TeamMembershipHandler();
        var isManager = new ManagerRoleHandler();

        userExists.SetNext(hasTeam).SetNext(isManager);

        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "user@example.com",
                DisplayName = "User",
                TeamId = null, // No team - will fail here
                IsManager = false
            }
        };

        // Act
        var result = await userExists.HandleAsync(context);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal("User must be part of a team", result.FailureReason);
        Assert.Equal("TEAM_MEMBERSHIP_REQUIRED", result.FailureCode);
    }

    [Fact]
    public void AuthorizationChainFactory_CreateViewTeamPendingVacationsChain_CreatesCorrectChain()
    {
        // Arrange & Act
        var chain = AuthorizationChainFactory.CreateViewTeamPendingVacationsChain();

        // Assert
        Assert.NotNull(chain);
        Assert.IsType<UserExistsHandler>(chain);
    }

    [Fact]
    public async Task AuthorizationService_WithValidChain_ExecutesCorrectly()
    {
        // Arrange
        var service = new AuthorizationService();
        var chain = new UserExistsHandler();
        var context = new AuthorizationContext
        {
            User = new User
            {
                Id = Guid.NewGuid(),
                EntraId = "entra-123",
                Email = "test@example.com",
                DisplayName = "Test User"
            }
        };

        // Act
        var result = await service.AuthorizeAsync(chain, context);

        // Assert
        Assert.True(result.IsAuthorized);
    }
}
