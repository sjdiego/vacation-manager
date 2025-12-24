using Xunit;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Validators;

namespace VacationManager.Tests.Core.Validators;

public class VacationValidatorsTests
{
    [Fact]
    public void CreateVacationDtoValidator_WithValidData_Passes()
    {
        // Arrange
        var validator = new CreateVacationDtoValidator();
        var dto = new CreateVacationDto
        {
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(6),
            Type = VacationType.Vacation,
            Notes = "Summer vacation"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateVacationDtoValidator_WithInvalidDateRange_Fails()
    {
        // Arrange
        var validator = new CreateVacationDtoValidator();
        var dto = new CreateVacationDto
        {
            StartDate = DateTime.UtcNow.AddDays(6),
            EndDate = DateTime.UtcNow.AddDays(1),
            Type = VacationType.Vacation
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartDate");
    }

    [Fact]
    public void CreateVacationDtoValidator_WithEmptyStartDate_Fails()
    {
        // Arrange
        var validator = new CreateVacationDtoValidator();
        var dto = new CreateVacationDto
        {
            StartDate = default,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateVacationDtoValidator_WithTooLongNotes_Fails()
    {
        // Arrange
        var validator = new CreateVacationDtoValidator();
        var dto = new CreateVacationDto
        {
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(6),
            Type = VacationType.Vacation,
            Notes = new string('a', 1001)
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateVacationDtoValidator_WithValidData_Passes()
    {
        // Arrange
        var validator = new UpdateVacationDtoValidator();
        var dto = new UpdateVacationDto
        {
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(6),
            Type = VacationType.SickLeave,
            Notes = "Medical appointment"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ApproveVacationDtoValidator_WithApproval_Passes()
    {
        // Arrange
        var validator = new ApproveVacationDtoValidator();
        var dto = new ApproveVacationDto
        {
            Approved = true
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ApproveVacationDtoValidator_WithRejectionAndReason_Passes()
    {
        // Arrange
        var validator = new ApproveVacationDtoValidator();
        var dto = new ApproveVacationDto
        {
            Approved = false,
            RejectReason = "Insufficient coverage"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ApproveVacationDtoValidator_WithRejectionButNoReason_Fails()
    {
        // Arrange
        var validator = new ApproveVacationDtoValidator();
        var dto = new ApproveVacationDto
        {
            Approved = false,
            RejectReason = null
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ApproveVacationDtoValidator_WithTooLongRejectReason_Fails()
    {
        // Arrange
        var validator = new ApproveVacationDtoValidator();
        var dto = new ApproveVacationDto
        {
            Approved = false,
            RejectReason = new string('a', 501)
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }
}

public class UserValidatorsTests
{
    [Fact]
    public void CreateUserDtoValidator_WithValidData_Passes()
    {
        // Arrange
        var validator = new CreateUserDtoValidator();
        var dto = new CreateUserDto
        {
            Email = "john@example.com",
            DisplayName = "John Doe",
            Department = "Engineering",
            TeamId = Guid.NewGuid()
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_WithInvalidEmail_Fails()
    {
        // Arrange
        var validator = new CreateUserDtoValidator();
        var dto = new CreateUserDto
        {
            Email = "invalid-email",
            DisplayName = "John Doe"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void CreateUserDtoValidator_WithEmptyEmail_Fails()
    {
        // Arrange
        var validator = new CreateUserDtoValidator();
        var dto = new CreateUserDto
        {
            Email = "",
            DisplayName = "John Doe"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateUserDtoValidator_WithShortDisplayName_Fails()
    {
        // Arrange
        var validator = new CreateUserDtoValidator();
        var dto = new CreateUserDto
        {
            Email = "john@example.com",
            DisplayName = "J"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateUserDtoValidator_WithValidData_Passes()
    {
        // Arrange
        var validator = new UpdateUserDtoValidator();
        var dto = new UpdateUserDto
        {
            DisplayName = "Jane Doe",
            Department = "Marketing",
            TeamId = Guid.NewGuid()
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateUserDtoValidator_WithNullFields_Passes()
    {
        // Arrange
        var validator = new UpdateUserDtoValidator();
        var dto = new UpdateUserDto
        {
            DisplayName = null,
            Department = null,
            TeamId = null
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }
}

public class TeamValidatorsTests
{
    [Fact]
    public void CreateTeamDtoValidator_WithValidData_Passes()
    {
        // Arrange
        var validator = new CreateTeamDtoValidator();
        var dto = new CreateTeamDto
        {
            Name = "Engineering",
            Description = "Software development team"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateTeamDtoValidator_WithEmptyName_Fails()
    {
        // Arrange
        var validator = new CreateTeamDtoValidator();
        var dto = new CreateTeamDto
        {
            Name = "",
            Description = "Team description"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTeamDtoValidator_WithShortName_Fails()
    {
        // Arrange
        var validator = new CreateTeamDtoValidator();
        var dto = new CreateTeamDto
        {
            Name = "E",
            Description = "Team description"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateTeamDtoValidator_WithTooLongDescription_Fails()
    {
        // Arrange
        var validator = new CreateTeamDtoValidator();
        var dto = new CreateTeamDto
        {
            Name = "Engineering",
            Description = new string('a', 1001)
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateTeamDtoValidator_WithValidData_Passes()
    {
        // Arrange
        var validator = new UpdateTeamDtoValidator();
        var dto = new UpdateTeamDto
        {
            Name = "Updated Engineering",
            Description = "Updated description"
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateTeamDtoValidator_WithNullFields_Passes()
    {
        // Arrange
        var validator = new UpdateTeamDtoValidator();
        var dto = new UpdateTeamDto
        {
            Name = null,
            Description = null
        };

        // Act
        var result = validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }
}
