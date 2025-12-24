using Xunit;
using VacationManager.Core.DTOs;

namespace VacationManager.Tests.Core.DTOs;

public class UserDtoTests
{
    [Fact]
    public void UserDto_Create()
    {
        // Arrange & Act
        var dto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            DisplayName = "John Doe",
            Department = "Engineering",
            TeamId = Guid.NewGuid(),
            IsManager = false,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("john@example.com", dto.Email);
        Assert.Equal("John Doe", dto.DisplayName);
        Assert.Equal("Engineering", dto.Department);
        Assert.NotNull(dto.TeamId);
        Assert.False(dto.IsManager);
    }

    [Fact]
    public void UserDto_CanHaveNullDepartment()
    {
        // Arrange & Act
        var dto = new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "jane@example.com",
            DisplayName = "Jane Doe",
            Department = null,
            TeamId = null
        };

        // Assert
        Assert.Null(dto.Department);
        Assert.Null(dto.TeamId);
    }

    [Fact]
    public void UserDto_DefaultEmptyStrings()
    {
        // Arrange & Act
        var dto = new UserDto();

        // Assert
        Assert.Equal(string.Empty, dto.Email);
        Assert.Equal(string.Empty, dto.DisplayName);
    }

    [Fact]
    public void CreateUserDto_WithRequired()
    {
        // Arrange & Act
        var dto = new CreateUserDto
        {
            Email = "john@example.com",
            DisplayName = "John Doe"
        };

        // Assert
        Assert.Equal("john@example.com", dto.Email);
        Assert.Equal("John Doe", dto.DisplayName);
        Assert.Null(dto.Department);
        Assert.Null(dto.TeamId);
    }

    [Fact]
    public void CreateUserDto_WithAllFields()
    {
        // Arrange
        var teamId = Guid.NewGuid();

        // Act
        var dto = new CreateUserDto
        {
            Email = "john@example.com",
            DisplayName = "John Doe",
            Department = "Engineering",
            TeamId = teamId
        };

        // Assert
        Assert.Equal("john@example.com", dto.Email);
        Assert.Equal("John Doe", dto.DisplayName);
        Assert.Equal("Engineering", dto.Department);
        Assert.Equal(teamId, dto.TeamId);
    }

    [Fact]
    public void UpdateUserDto_Create()
    {
        // Arrange & Act
        var dto = new UpdateUserDto
        {
            DisplayName = "Jane Doe",
            Department = "Marketing",
            TeamId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal("Jane Doe", dto.DisplayName);
        Assert.Equal("Marketing", dto.Department);
        Assert.NotNull(dto.TeamId);
    }

    [Fact]
    public void UpdateUserDto_CanHaveNullFields()
    {
        // Arrange & Act
        var dto = new UpdateUserDto
        {
            DisplayName = null,
            Department = null,
            TeamId = null
        };

        // Assert
        Assert.Null(dto.DisplayName);
        Assert.Null(dto.Department);
        Assert.Null(dto.TeamId);
    }

    [Fact]
    public void UpdateUserDto_PartialUpdate()
    {
        // Arrange & Act
        var dto = new UpdateUserDto
        {
            DisplayName = "Updated Name"
        };

        // Assert
        Assert.Equal("Updated Name", dto.DisplayName);
        Assert.Null(dto.Department);
        Assert.Null(dto.TeamId);
    }
}
