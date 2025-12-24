using Xunit;
using VacationManager.Core.Entities;

namespace VacationManager.Tests.Core.Entities;

public class TeamEntityTests
{
    [Fact]
    public void Team_Create_WithRequired()
    {
        // Arrange & Act
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Engineering"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, team.Id);
        Assert.Equal("Engineering", team.Name);
    }

    [Fact]
    public void Team_Create_WithDescription()
    {
        // Arrange & Act
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
            Description = "Software development team"
        };

        // Assert
        Assert.Equal("Engineering", team.Name);
        Assert.Equal("Software development team", team.Description);
    }

    [Fact]
    public void Team_HasMembersCollection()
    {
        // Arrange
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Engineering"
        };

        // Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "john@example.com",
            DisplayName = "John Doe",
            TeamId = team.Id
        };
        team.Members.Add(user);

        // Assert
        Assert.Single(team.Members);
        Assert.Contains(user, team.Members);
    }

    [Fact]
    public void Team_CanAddMultipleMembers()
    {
        // Arrange
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Engineering"
        };

        // Act
        for (int i = 0; i < 5; i++)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                EntraId = $"entra-{i}",
                Email = $"user{i}@example.com",
                DisplayName = $"User {i}",
                TeamId = team.Id
            };
            team.Members.Add(user);
        }

        // Assert
        Assert.Equal(5, team.Members.Count);
    }

    [Fact]
    public void Team_DefaultMembersCollectionIsEmpty()
    {
        // Arrange & Act
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Engineering"
        };

        // Assert
        Assert.NotNull(team.Members);
        Assert.Empty(team.Members);
    }
}
