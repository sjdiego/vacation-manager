using Xunit;
using VacationManager.Core.Entities;
using VacationManager.Data;
using VacationManager.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace VacationManager.Tests.Data.Repositories;

public class VacationRepositoryTests : IDisposable
{
    private readonly VacationManagerDbContext _context;
    private readonly VacationRepository _repository;

    public VacationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<VacationManagerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VacationManagerDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new VacationRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingVacation_ReturnsVacation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraId = "entra-1", Email = "user@example.com", DisplayName = "User" };
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Status = VacationStatus.Approved
        };
        await _context.Users.AddAsync(user);
        await _context.Vacations.AddAsync(vacation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(vacation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(vacation.Id, result.Id);
        Assert.Equal(vacation.UserId, result.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentVacation_ReturnsNull()
    {
        // Arrange
        var vacationId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(vacationId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingUser_ReturnsVacations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraId = "entra-1", Email = "user@example.com", DisplayName = "User" };
        var vacations = new[]
        {
            new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), User = user },
            new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(15), User = user }
        };
        await _context.Users.AddAsync(user);
        await _context.Vacations.AddRangeAsync(vacations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, v => Assert.Equal(userId, v.UserId));
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNoVacations_ReturnsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTeamAsync_WithExistingTeam_ReturnsTeamVacations()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), EntraId = "entra-1", Email = "user@example.com", DisplayName = "User", TeamId = teamId };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var vacations = new[]
        {
            new Vacation { Id = Guid.NewGuid(), UserId = user.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), User = user },
            new Vacation { Id = Guid.NewGuid(), UserId = user.Id, StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(15), User = user }
        };
        await _context.Vacations.AddRangeAsync(vacations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTeamAsync(teamId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetPendingAsync_WithPendingVacations_ReturnsPendingOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraId = "entra-1", Email = "user@example.com", DisplayName = "User" };
        var vacations = new[]
        {
            new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), Status = VacationStatus.Pending, User = user },
            new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(15), Status = VacationStatus.Approved, User = user }
        };
        await _context.Users.AddAsync(user);
        await _context.Vacations.AddRangeAsync(vacations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingAsync();

        // Assert
        Assert.Single(result);
        Assert.All(result, v => Assert.Equal(VacationStatus.Pending, v.Status));
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithOverlappingVacations_ReturnsMatching()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraId = "entra-1", Email = "user@example.com", DisplayName = "User" };
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(10);

        var vacations = new[]
        {
            new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = startDate.AddDays(2), EndDate = startDate.AddDays(5), Status = VacationStatus.Approved, User = user },
            new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = startDate.AddDays(15), EndDate = startDate.AddDays(20), Status = VacationStatus.Approved, User = user }
        };
        await _context.Users.AddAsync(user);
        await _context.Vacations.AddRangeAsync(vacations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDateRangeAsync(startDate, endDate);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidVacation_CreatesVacation()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Status = VacationStatus.Pending
        };

        // Act
        var result = await _repository.CreateAsync(vacation);

        // Assert
        Assert.Equal(vacation.Id, result.Id);
        var createdVacation = await _context.Vacations.FindAsync(vacation.Id);
        Assert.NotNull(createdVacation);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingVacation_UpdatesVacation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraId = "entra-1", Email = "user@example.com", DisplayName = "User" };
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Status = VacationStatus.Pending
        };
        await _context.Users.AddAsync(user);
        await _context.Vacations.AddAsync(vacation);
        await _context.SaveChangesAsync();

        // Act
        vacation.Status = VacationStatus.Approved;
        var result = await _repository.UpdateAsync(vacation);

        // Assert
        Assert.Equal(VacationStatus.Approved, result.Status);
        var updatedVacation = await _context.Vacations.FindAsync(vacation.Id);
        Assert.Equal(VacationStatus.Approved, updatedVacation?.Status);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingVacation_DeletesVacation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraId = "entra-1", Email = "user@example.com", DisplayName = "User" };
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5)
        };
        await _context.Users.AddAsync(user);
        await _context.Vacations.AddAsync(vacation);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(vacation.Id);

        // Assert
        var deletedVacation = await _context.Vacations.FindAsync(vacation.Id);
        Assert.Null(deletedVacation);
    }

    [Fact]
    public async Task DeleteAsync_WithNonexistentVacation_DoesNotThrow()
    {
        // Arrange
        var vacationId = Guid.NewGuid();

        // Act & Assert - Should not throw
        await _repository.DeleteAsync(vacationId);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
