using Xunit;
using VacationManager.Core.Entities;
using VacationManager.Data;
using VacationManager.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace VacationManager.Tests.Data.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly VacationManagerDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<VacationManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VacationManagerDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "user@example.com",
            DisplayName = "John Doe"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("John Doe", result.DisplayName);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEntraIdAsync_WithExistingEntraId_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "user@example.com",
            DisplayName = "John Doe"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEntraIdAsync("entra-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("entra-123", result.EntraId);
    }

    [Fact]
    public async Task GetByEntraIdAsync_WithNonexistentEntraId_ReturnsNull()
    {
        // Arrange
        var entraId = "nonexistent-entra-id";

        // Act
        var result = await _repository.GetByEntraIdAsync(entraId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "user@example.com",
            DisplayName = "John Doe"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("user@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonexistentEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleUsers_ReturnsAllUsersOrdered()
    {
        // Arrange
        var users = new[]
        {
            new User { Id = Guid.NewGuid(), EntraId = "entra-1", Email = "alice@example.com", DisplayName = "Alice" },
            new User { Id = Guid.NewGuid(), EntraId = "entra-2", Email = "bob@example.com", DisplayName = "Bob" },
            new User { Id = Guid.NewGuid(), EntraId = "entra-3", Email = "charlie@example.com", DisplayName = "Charlie" }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
        var orderedNames = result.Select(u => u.DisplayName).ToList();
        Assert.Equal(new[] { "Alice", "Bob", "Charlie" }, orderedNames);
    }

    [Fact]
    public async Task GetAllAsync_WithNoUsers_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTeamAsync_WithExistingTeam_ReturnsTeamMembers()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var users = new[]
        {
            new User { Id = Guid.NewGuid(), EntraId = "entra-1", Email = "user1@example.com", DisplayName = "User One", TeamId = teamId },
            new User { Id = Guid.NewGuid(), EntraId = "entra-2", Email = "user2@example.com", DisplayName = "User Two", TeamId = teamId },
            new User { Id = Guid.NewGuid(), EntraId = "entra-3", Email = "user3@example.com", DisplayName = "User Three", TeamId = null }
        };
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTeamAsync(teamId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.Equal(teamId, u.TeamId));
    }

    [Fact]
    public async Task GetByTeamAsync_WithNoMembers_ReturnsEmpty()
    {
        // Arrange
        var teamId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByTeamAsync(teamId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidUser_CreatesUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "newuser@example.com",
            DisplayName = "New User"
        };

        // Act
        var result = await _repository.CreateAsync(user);

        // Assert
        Assert.Equal(user.Id, result.Id);
        var createdUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(createdUser);
        Assert.Equal("newuser@example.com", createdUser?.Email);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingUser_UpdatesUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "user@example.com",
            DisplayName = "Original Name"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        user.DisplayName = "Updated Name";
        user.Department = "Engineering";
        var result = await _repository.UpdateAsync(user);

        // Assert
        Assert.Equal("Updated Name", result.DisplayName);
        Assert.Equal("Engineering", result.Department);
        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.Equal("Updated Name", updatedUser?.DisplayName);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingUser_DeletesUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "user@example.com",
            DisplayName = "User To Delete"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(user.Id);

        // Assert
        var deletedUser = await _context.Users.FindAsync(user.Id);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task DeleteAsync_WithNonexistentUser_DoesNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert - Should not throw
        await _repository.DeleteAsync(userId);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
