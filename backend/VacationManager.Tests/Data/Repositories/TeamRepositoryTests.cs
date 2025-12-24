using Xunit;
using VacationManager.Core.Entities;
using VacationManager.Data;
using VacationManager.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace VacationManager.Tests.Data.Repositories;

public class TeamRepositoryTests : IDisposable
{
    private readonly VacationManagerDbContext _context;
    private readonly TeamRepository _repository;

    public TeamRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<VacationManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VacationManagerDbContext(options);
        _repository = new TeamRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingTeam_ReturnsTeam()
    {
        // Arrange
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
            Description = "Software development team"
        };
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(team.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(team.Id, result.Id);
        Assert.Equal("Engineering", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentTeam_ReturnsNull()
    {
        // Arrange
        var teamId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(teamId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithMembers_IncludesMembers()
    {
        // Arrange
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Engineering"
        };
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-1",
            Email = "user@example.com",
            DisplayName = "Team Member",
            TeamId = team.Id
        };
        await _context.Teams.AddAsync(team);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(team.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!.Members);
        Assert.Equal("Team Member", result.Members.First().DisplayName);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleTeams_ReturnsAllTeamsOrdered()
    {
        // Arrange
        var teams = new[]
        {
            new Team { Id = Guid.NewGuid(), Name = "Zebra Team" },
            new Team { Id = Guid.NewGuid(), Name = "Alpha Team" },
            new Team { Id = Guid.NewGuid(), Name = "Beta Team" }
        };
        await _context.Teams.AddRangeAsync(teams);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
        var orderedNames = result.Select(t => t.Name).ToList();
        Assert.Equal(new[] { "Alpha Team", "Beta Team", "Zebra Team" }, orderedNames);
    }

    [Fact]
    public async Task GetAllAsync_WithNoTeams_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMembers_IncludesMembers()
    {
        // Arrange
        var team = new Team { Id = Guid.NewGuid(), Name = "Engineering" };
        var users = new[]
        {
            new User { Id = Guid.NewGuid(), EntraId = "entra-1", Email = "user1@example.com", DisplayName = "User One", TeamId = team.Id },
            new User { Id = Guid.NewGuid(), EntraId = "entra-2", Email = "user2@example.com", DisplayName = "User Two", TeamId = team.Id }
        };
        await _context.Teams.AddAsync(team);
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var retrievedTeam = result.First();
        Assert.Equal(2, retrievedTeam.Members.Count);
    }

    [Fact]
    public async Task CreateAsync_WithValidTeam_CreatesTeam()
    {
        // Arrange
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "New Team",
            Description = "A new team"
        };

        // Act
        var result = await _repository.CreateAsync(team);

        // Assert
        Assert.Equal(team.Id, result.Id);
        var createdTeam = await _context.Teams.FindAsync(team.Id);
        Assert.NotNull(createdTeam);
        Assert.Equal("New Team", createdTeam?.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingTeam_UpdatesTeam()
    {
        // Arrange
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Original Team",
            Description = "Original description"
        };
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();

        // Act
        team.Name = "Updated Team";
        team.Description = "Updated description";
        var result = await _repository.UpdateAsync(team);

        // Assert
        Assert.Equal("Updated Team", result.Name);
        Assert.Equal("Updated description", result.Description);
        var updatedTeam = await _context.Teams.FindAsync(team.Id);
        Assert.Equal("Updated Team", updatedTeam?.Name);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingTeam_DeletesTeam()
    {
        // Arrange
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Team To Delete"
        };
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(team.Id);

        // Assert
        var deletedTeam = await _context.Teams.FindAsync(team.Id);
        Assert.Null(deletedTeam);
    }

    [Fact]
    public async Task DeleteAsync_WithNonexistentTeam_DoesNotThrow()
    {
        // Arrange
        var teamId = Guid.NewGuid();

        // Act & Assert - Should not throw
        await _repository.DeleteAsync(teamId);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
