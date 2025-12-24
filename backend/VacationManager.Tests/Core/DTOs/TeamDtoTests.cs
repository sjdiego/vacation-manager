using Xunit;
using VacationManager.Core.DTOs;

namespace VacationManager.Tests.Core.DTOs;

public class TeamDtoTests
{
    [Fact]
    public void TeamDto_Create()
    {
        // Arrange & Act
        var dto = new TeamDto
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
            Description = "Software development team",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("Engineering", dto.Name);
        Assert.Equal("Software development team", dto.Description);
    }

    [Fact]
    public void TeamDto_CanHaveNullDescription()
    {
        // Arrange & Act
        var dto = new TeamDto
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
            Description = null
        };

        // Assert
        Assert.Null(dto.Description);
    }

    [Fact]
    public void TeamDto_DefaultEmptyString()
    {
        // Arrange & Act
        var dto = new TeamDto();

        // Assert
        Assert.Equal(string.Empty, dto.Name);
    }

    [Fact]
    public void CreateTeamDto_WithRequired()
    {
        // Arrange & Act
        var dto = new CreateTeamDto
        {
            Name = "Engineering"
        };

        // Assert
        Assert.Equal("Engineering", dto.Name);
        Assert.Null(dto.Description);
    }

    [Fact]
    public void CreateTeamDto_WithDescription()
    {
        // Arrange & Act
        var dto = new CreateTeamDto
        {
            Name = "Engineering",
            Description = "Building great software"
        };

        // Assert
        Assert.Equal("Engineering", dto.Name);
        Assert.Equal("Building great software", dto.Description);
    }

    [Fact]
    public void UpdateTeamDto_Create()
    {
        // Arrange & Act
        var dto = new UpdateTeamDto
        {
            Name = "Updated Engineering",
            Description = "Updated description"
        };

        // Assert
        Assert.Equal("Updated Engineering", dto.Name);
        Assert.Equal("Updated description", dto.Description);
    }

    [Fact]
    public void UpdateTeamDto_CanHaveNullFields()
    {
        // Arrange & Act
        var dto = new UpdateTeamDto
        {
            Name = null,
            Description = null
        };

        // Assert
        Assert.Null(dto.Name);
        Assert.Null(dto.Description);
    }

    [Fact]
    public void UpdateTeamDto_PartialUpdate()
    {
        // Arrange & Act
        var dto = new UpdateTeamDto
        {
            Name = "New Name"
        };

        // Assert
        Assert.Equal("New Name", dto.Name);
        Assert.Null(dto.Description);
    }

    [Fact]
    public void UpdateTeamDto_OnlyDescriptionUpdate()
    {
        // Arrange & Act
        var dto = new UpdateTeamDto
        {
            Description = "New description only"
        };

        // Assert
        Assert.Null(dto.Name);
        Assert.Equal("New description only", dto.Description);
    }
}
