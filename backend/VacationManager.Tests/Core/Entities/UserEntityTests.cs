using Xunit;
using VacationManager.Core.Entities;

namespace VacationManager.Tests.Core.Entities;

public class UserEntityTests
{
    [Fact]
    public void User_Create_WithRequired()
    {
        // Arrange & Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "john@example.com",
            DisplayName = "John Doe"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("entra-123", user.EntraId);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal("John Doe", user.DisplayName);
        Assert.False(user.IsManager);
    }

    [Fact]
    public void User_Create_WithOptionalFields()
    {
        // Arrange & Act
        var teamId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "john@example.com",
            DisplayName = "John Doe",
            Department = "Engineering",
            TeamId = teamId,
            IsManager = true
        };

        // Assert
        Assert.Equal("Engineering", user.Department);
        Assert.Equal(teamId, user.TeamId);
        Assert.True(user.IsManager);
    }

    [Fact]
    public void User_HasVacationsCollection()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "john@example.com",
            DisplayName = "John Doe"
        };

        // Act
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5)
        };
        user.Vacations.Add(vacation);

        // Assert
        Assert.Single(user.Vacations);
        Assert.Contains(vacation, user.Vacations);
    }

    [Fact]
    public void User_HasApprovedVacationsCollection()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "john@example.com",
            DisplayName = "John Doe",
            IsManager = true
        };

        // Act
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            ApprovedBy = user.Id,
            Status = VacationStatus.Approved
        };
        user.ApprovedVacations.Add(vacation);

        // Assert
        Assert.Single(user.ApprovedVacations);
        Assert.Contains(vacation, user.ApprovedVacations);
    }

    [Fact]
    public void User_HasTeamRelationship()
    {
        // Arrange
        var team = new Team { Id = Guid.NewGuid(), Name = "Engineering" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "john@example.com",
            DisplayName = "John Doe",
            TeamId = team.Id,
            Team = team
        };

        // Assert
        Assert.NotNull(user.Team);
        Assert.Equal(team.Id, user.Team.Id);
        Assert.Equal("Engineering", user.Team.Name);
    }

    [Fact]
    public void User_DefaultIsNotManager()
    {
        // Arrange & Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "john@example.com",
            DisplayName = "John Doe"
        };

        // Assert
        Assert.False(user.IsManager);
    }
}
