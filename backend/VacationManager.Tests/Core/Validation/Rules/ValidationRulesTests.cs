using Xunit;
using NSubstitute;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;
using VacationManager.Core.Validation.Rules;

namespace VacationManager.Tests.Core.Validation.Rules;

public class TeamMembershipValidationRuleTests
{
    private readonly TeamMembershipValidationRule _rule;

    public TeamMembershipValidationRuleTests()
    {
        _rule = new TeamMembershipValidationRule();
    }

    [Fact]
    public async Task ValidateAsync_WithTeamMember_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "john@example.com",
            DisplayName = "John Doe",
            TeamId = Guid.NewGuid()
        };

        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation
        };

        // Act
        var result = await _rule.ValidateAsync(vacation, user);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_WithoutTeamMember_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-456",
            Email = "john@example.com",
            DisplayName = "John Doe",
            TeamId = null
        };

        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation
        };

        // Act
        var result = await _rule.ValidateAsync(vacation, user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("User must be part of a team to request vacation", result.ErrorMessage);
        Assert.Equal("TEAM_MEMBERSHIP_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public void Order_ReturnsCorrectPriority()
    {
        // Assert
        Assert.Equal(1, _rule.Order);
    }
}

public class VacationOverlapValidationRuleTests
{
    private readonly IVacationRepository _vacationRepository;
    private readonly VacationOverlapValidationRule _rule;

    public VacationOverlapValidationRuleTests()
    {
        _vacationRepository = Substitute.For<IVacationRepository>();
        _rule = new VacationOverlapValidationRule(_vacationRepository);
    }

    [Fact]
    public async Task ValidateAsync_WithNoOverlap_ReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), EntraId = "entra-123", Email = "test@example.com", DisplayName = "Test User", TeamId = Guid.NewGuid() };
        var newVacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(15),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var existingVacations = new List<Vacation>
        {
            new() { Id = Guid.NewGuid(), UserId = user.Id, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(5), Status = VacationStatus.Approved }
        };

        _vacationRepository.GetByUserIdAsync(user.Id).Returns(existingVacations);

        // Act
        var result = await _rule.ValidateAsync(newVacation, user);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_WithOverlappingApprovedVacation_ReturnsFailure()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), EntraId = "entra-123", Email = "test@example.com", DisplayName = "Test User", TeamId = Guid.NewGuid() };
        var newVacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(8),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var existingVacations = new List<Vacation>
        {
            new() { Id = Guid.NewGuid(), UserId = user.Id, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(5), Status = VacationStatus.Approved }
        };

        _vacationRepository.GetByUserIdAsync(user.Id).Returns(existingVacations);

        // Act
        var result = await _rule.ValidateAsync(newVacation, user);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("You have overlapping approved vacations in this date range", result.ErrorMessage);
        Assert.Equal("VACATION_OVERLAP", result.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_WithOverlappingPendingVacation_ReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), EntraId = "entra-123", Email = "test@example.com", DisplayName = "Test User", TeamId = Guid.NewGuid() };
        var newVacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(8),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var existingVacations = new List<Vacation>
        {
            new() { Id = Guid.NewGuid(), UserId = user.Id, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(5), Status = VacationStatus.Pending }
        };

        _vacationRepository.GetByUserIdAsync(user.Id).Returns(existingVacations);

        // Act
        var result = await _rule.ValidateAsync(newVacation, user);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_WhenUpdatingExistingVacation_IgnoresSelf()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), EntraId = "entra-123", Email = "test@example.com", DisplayName = "Test User", TeamId = Guid.NewGuid() };
        var vacation = new Vacation
        {
            Id = vacationId,
            UserId = user.Id,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var existingVacations = new List<Vacation>
        {
            new() { Id = vacationId, UserId = user.Id, StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(5), Status = VacationStatus.Approved }
        };

        _vacationRepository.GetByUserIdAsync(user.Id).Returns(existingVacations);

        // Act
        var result = await _rule.ValidateAsync(vacation, user);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Order_ReturnsCorrectPriority()
    {
        // Assert
        Assert.Equal(2, _rule.Order);
    }
}
