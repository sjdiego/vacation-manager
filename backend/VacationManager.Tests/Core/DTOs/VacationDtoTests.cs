using Xunit;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;

namespace VacationManager.Tests.Core.DTOs;

public class VacationDtoTests
{
    [Fact]
    public void VacationDto_Create()
    {
        // Arrange & Act
        var dto = new VacationDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = "John Doe",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Approved,
            ApprovedBy = Guid.NewGuid(),
            Notes = "Summer vacation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.NotEqual(Guid.Empty, dto.UserId);
        Assert.Equal("John Doe", dto.UserName);
        Assert.Equal(VacationType.Vacation, dto.Type);
        Assert.Equal(VacationStatus.Approved, dto.Status);
        Assert.Equal("Summer vacation", dto.Notes);
    }

    [Fact]
    public void VacationDto_CanHaveNullUserName()
    {
        // Arrange & Act
        var dto = new VacationDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = null,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        // Assert
        Assert.Null(dto.UserName);
    }

    [Fact]
    public void VacationDto_CanHaveNullApprovedBy()
    {
        // Arrange & Act
        var dto = new VacationDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            ApprovedBy = null
        };

        // Assert
        Assert.Null(dto.ApprovedBy);
    }

    [Fact]
    public void CreateVacationDto_HasDefaults()
    {
        // Arrange & Act
        var dto = new CreateVacationDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5)
        };

        // Assert
        Assert.Equal(VacationType.Vacation, dto.Type);
        Assert.Null(dto.Notes);
    }

    [Fact]
    public void CreateVacationDto_CanSetCustomType()
    {
        // Arrange & Act
        var dto = new CreateVacationDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Type = VacationType.SickLeave,
            Notes = "Medical appointment"
        };

        // Assert
        Assert.Equal(VacationType.SickLeave, dto.Type);
        Assert.Equal("Medical appointment", dto.Notes);
    }

    [Fact]
    public void UpdateVacationDto_Create()
    {
        // Arrange & Act
        var dto = new UpdateVacationDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3),
            Type = VacationType.PersonalDay,
            Notes = "Updated notes"
        };

        // Assert
        Assert.Equal(VacationType.PersonalDay, dto.Type);
        Assert.Equal("Updated notes", dto.Notes);
    }

    [Fact]
    public void ApproveVacationDto_Approved()
    {
        // Arrange & Act
        var dto = new ApproveVacationDto
        {
            Approved = true
        };

        // Assert
        Assert.True(dto.Approved);
        Assert.Null(dto.RejectReason);
    }

    [Fact]
    public void ApproveVacationDto_Rejected()
    {
        // Arrange & Act
        var dto = new ApproveVacationDto
        {
            Approved = false,
            RejectReason = "Insufficient coverage"
        };

        // Assert
        Assert.False(dto.Approved);
        Assert.Equal("Insufficient coverage", dto.RejectReason);
    }
}
