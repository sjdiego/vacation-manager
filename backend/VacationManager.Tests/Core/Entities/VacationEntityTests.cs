using Xunit;
using VacationManager.Core.Entities;

namespace VacationManager.Tests.Core.Entities;

public class VacationEntityTests
{
    [Fact]
    public void Vacation_Create_WithDefaults()
    {
        // Arrange & Act
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        // Assert
        Assert.NotEqual(Guid.Empty, vacation.Id);
        Assert.NotEqual(Guid.Empty, vacation.UserId);
        Assert.Equal(VacationType.Vacation, vacation.Type);
        Assert.Equal(VacationStatus.Pending, vacation.Status);
        Assert.Null(vacation.ApprovedBy);
        Assert.Null(vacation.Notes);
    }

    [Fact]
    public void Vacation_Create_WithCustomType()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(3);

        // Act
        var vacation = new Vacation
        {
            Id = vacationId,
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate,
            Type = VacationType.SickLeave,
            Notes = "Doctor's appointment"
        };

        // Assert
        Assert.Equal(vacationId, vacation.Id);
        Assert.Equal(userId, vacation.UserId);
        Assert.Equal(startDate, vacation.StartDate);
        Assert.Equal(endDate, vacation.EndDate);
        Assert.Equal(VacationType.SickLeave, vacation.Type);
        Assert.Equal("Doctor's appointment", vacation.Notes);
    }

    [Fact]
    public void Vacation_CanUpdateStatus()
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
        vacation.Status = VacationStatus.Approved;
        vacation.ApprovedBy = Guid.NewGuid();

        // Assert
        Assert.Equal(VacationStatus.Approved, vacation.Status);
        Assert.NotNull(vacation.ApprovedBy);
    }

    [Fact]
    public void Vacation_HasUserRelationship()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "user@example.com",
            DisplayName = "John Doe"
        };
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            User = user
        };

        // Assert
        Assert.NotNull(vacation.User);
        Assert.Equal(user.Id, vacation.User.Id);
        Assert.Equal("John Doe", vacation.User.DisplayName);
    }

    [Theory]
    [InlineData(VacationType.Vacation)]
    [InlineData(VacationType.SickLeave)]
    [InlineData(VacationType.PersonalDay)]
    [InlineData(VacationType.CompensatoryTime)]
    [InlineData(VacationType.Other)]
    public void Vacation_AllVacationTypes_AreValid(VacationType type)
    {
        // Arrange & Act
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Type = type
        };

        // Assert
        Assert.Equal(type, vacation.Type);
    }
}
