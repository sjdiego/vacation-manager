using Xunit;
using VacationManager.Core.Entities;
using VacationManager.Core.Specifications;

namespace VacationManager.Tests.Core.Specifications;

public class VacationSpecificationsTests
{
    [Fact]
    public void DateRangeSpecification_WithOverlappingDates_ReturnsTrue()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = new DateTime(2025, 1, 10),
            EndDate = new DateTime(2025, 1, 15),
            Type = VacationType.Vacation,
            Status = VacationStatus.Approved
        };

        var spec = new DateRangeSpecification(new DateTime(2025, 1, 12), new DateTime(2025, 1, 20));

        // Act
        var result = spec.IsSatisfiedBy(vacation);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DateRangeSpecification_WithNonOverlappingDates_ReturnsFalse()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = new DateTime(2025, 1, 10),
            EndDate = new DateTime(2025, 1, 15),
            Type = VacationType.Vacation,
            Status = VacationStatus.Approved
        };

        var spec = new DateRangeSpecification(new DateTime(2025, 1, 20), new DateTime(2025, 1, 25));

        // Act
        var result = spec.IsSatisfiedBy(vacation);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VacationStatusSpecification_WithMatchingStatus_ReturnsTrue()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var spec = new VacationStatusSpecification(VacationStatus.Pending);

        // Act
        var result = spec.IsSatisfiedBy(vacation);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VacationStatusSpecification_WithDifferentStatus_ReturnsFalse()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Approved
        };

        var spec = new VacationStatusSpecification(VacationStatus.Pending);

        // Act
        var result = spec.IsSatisfiedBy(vacation);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PendingVacationsSpecification_WithPendingVacation_ReturnsTrue()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var spec = new PendingVacationsSpecification();

        // Act
        var result = spec.IsSatisfiedBy(vacation);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ApprovedVacationsSpecification_WithApprovedVacation_ReturnsTrue()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Approved
        };

        var spec = new ApprovedVacationsSpecification();

        // Act
        var result = spec.IsSatisfiedBy(vacation);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UserSpecification_WithMatchingUser_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var spec = new UserSpecification(userId);

        // Act
        var result = spec.IsSatisfiedBy(vacation);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VacationTypeSpecification_WithMatchingType_ReturnsTrue()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.SickLeave,
            Status = VacationStatus.Pending
        };

        var spec = new VacationTypeSpecification(VacationType.SickLeave);

        // Act
        var result = spec.IsSatisfiedBy(vacation);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AndSpecification_WithBothSatisfied_ReturnsTrue()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = new DateTime(2025, 1, 10),
            EndDate = new DateTime(2025, 1, 15),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var dateSpec = new DateRangeSpecification(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
        var statusSpec = new PendingVacationsSpecification();
        var combinedSpec = dateSpec.And(statusSpec);

        // Act
        var result = combinedSpec.IsSatisfiedBy(vacation);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AndSpecification_WithOneSatisfied_ReturnsFalse()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = new DateTime(2025, 1, 10),
            EndDate = new DateTime(2025, 1, 15),
            Type = VacationType.Vacation,
            Status = VacationStatus.Approved
        };

        var dateSpec = new DateRangeSpecification(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
        var statusSpec = new PendingVacationsSpecification();
        var combinedSpec = dateSpec.And(statusSpec);

        // Act
        var result = combinedSpec.IsSatisfiedBy(vacation);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OrSpecification_WithOneSatisfied_ReturnsTrue()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var pendingSpec = new PendingVacationsSpecification();
        var approvedSpec = new ApprovedVacationsSpecification();
        var combinedSpec = pendingSpec.Or(approvedSpec);

        // Act
        var result = combinedSpec.IsSatisfiedBy(vacation);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NotSpecification_WithSatisfied_ReturnsFalse()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Pending
        };

        var pendingSpec = new PendingVacationsSpecification();
        var notPendingSpec = pendingSpec.Not();

        // Act
        var result = notPendingSpec.IsSatisfiedBy(vacation);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ComplexSpecification_WithMultipleConditions_WorksCorrectly()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = new DateTime(2025, 1, 10),
            EndDate = new DateTime(2025, 1, 15),
            Type = VacationType.Vacation,
            Status = VacationStatus.Approved
        };

        // (Approved OR Pending) AND DateRange AND NOT SickLeave
        var approvedOrPending = new ApprovedVacationsSpecification()
            .Or(new PendingVacationsSpecification());
        var inDateRange = new DateRangeSpecification(new DateTime(2025, 1, 1), new DateTime(2025, 1, 31));
        var notSickLeave = new VacationTypeSpecification(VacationType.SickLeave).Not();

        var complexSpec = approvedOrPending.And(inDateRange).And(notSickLeave);

        // Act
        var result = complexSpec.IsSatisfiedBy(vacation);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SpecificationExtensions_WhereWithSpecification_FiltersCorrectly()
    {
        // Arrange
        var vacations = new List<Vacation>
        {
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), Type = VacationType.Vacation, Status = VacationStatus.Pending },
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), Type = VacationType.Vacation, Status = VacationStatus.Approved },
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), Type = VacationType.Vacation, Status = VacationStatus.Pending }
        };

        var spec = new PendingVacationsSpecification();

        // Act
        var filtered = vacations.Where(spec).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, v => Assert.Equal(VacationStatus.Pending, v.Status));
    }
}
