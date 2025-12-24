using Xunit;
using AutoMapper;
using VacationManager.Api;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;

namespace VacationManager.Tests.Api;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    // Vacation Mappings
    [Fact]
    public void MapVacationToVacationDto_WithValidVacation_MapsCorrectly()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.Vacation,
            Status = VacationStatus.Approved,
            Notes = "Summer vacation",
            ApprovedBy = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var dto = _mapper.Map<VacationDto>(vacation);

        // Assert
        Assert.Equal(vacation.Id, dto.Id);
        Assert.Equal(vacation.UserId, dto.UserId);
        Assert.Equal(vacation.StartDate, dto.StartDate);
        Assert.Equal(vacation.EndDate, dto.EndDate);
        Assert.Equal(vacation.Type, dto.Type);
        Assert.Equal(vacation.Status, dto.Status);
        Assert.Equal(vacation.Notes, dto.Notes);
        Assert.Equal(vacation.ApprovedBy, dto.ApprovedBy);
    }

    [Fact]
    public void MapVacationToVacationDto_WithUserRelationship_MapsUserName()
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

        // Act
        var dto = _mapper.Map<VacationDto>(vacation);

        // Assert
        Assert.Equal("John Doe", dto.UserName);
    }

    [Fact]
    public void MapVacationToVacationDto_WithoutUser_UserNameIsNull()
    {
        // Arrange
        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            User = null
        };

        // Act
        var dto = _mapper.Map<VacationDto>(vacation);

        // Assert
        Assert.Null(dto.UserName);
    }

    [Fact]
    public void MapCreateVacationDtoToVacation_WithValidDto_MapsCorrectly()
    {
        // Arrange
        var dto = new CreateVacationDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Type = VacationType.SickLeave,
            Notes = "Medical appointment"
        };

        // Act
        var vacation = _mapper.Map<Vacation>(dto);

        // Assert
        Assert.Equal(dto.StartDate, vacation.StartDate);
        Assert.Equal(dto.EndDate, vacation.EndDate);
        Assert.Equal(dto.Type, vacation.Type);
        Assert.Equal(dto.Notes, vacation.Notes);
    }

    [Fact]
    public void MapUpdateVacationDtoToVacation_WithValidDto_MapsCorrectly()
    {
        // Arrange
        var dto = new UpdateVacationDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3),
            Type = VacationType.PersonalDay,
            Notes = "Updated notes"
        };

        // Act
        var vacation = _mapper.Map<Vacation>(dto);

        // Assert
        Assert.Equal(dto.StartDate, vacation.StartDate);
        Assert.Equal(dto.EndDate, vacation.EndDate);
        Assert.Equal(dto.Type, vacation.Type);
        Assert.Equal(dto.Notes, vacation.Notes);
    }

    // User Mappings
    [Fact]
    public void MapUserToUserDto_WithValidUser_MapsCorrectly()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraId = "entra-123",
            Email = "user@example.com",
            DisplayName = "John Doe",
            Department = "Engineering",
            TeamId = teamId,
            IsManager = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var dto = _mapper.Map<UserDto>(user);

        // Assert
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.Email, dto.Email);
        Assert.Equal(user.DisplayName, dto.DisplayName);
        Assert.Equal(user.Department, dto.Department);
        Assert.Equal(user.TeamId, dto.TeamId);
        Assert.Equal(user.IsManager, dto.IsManager);
    }

    [Fact]
    public void MapCreateUserDtoToUser_WithValidDto_MapsCorrectly()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var dto = new CreateUserDto
        {
            Email = "newuser@example.com",
            DisplayName = "New User",
            Department = "Marketing",
            TeamId = teamId
        };

        // Act
        var user = _mapper.Map<User>(dto);

        // Assert
        Assert.Equal(dto.Email, user.Email);
        Assert.Equal(dto.DisplayName, user.DisplayName);
        Assert.Equal(dto.Department, user.Department);
        Assert.Equal(dto.TeamId, user.TeamId);
    }

    [Fact]
    public void MapUpdateUserDtoToUser_WithValidDto_MapsCorrectly()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            DisplayName = "Updated Name",
            Department = "Sales",
            TeamId = Guid.NewGuid()
        };

        // Act
        var user = _mapper.Map<User>(dto);

        // Assert
        Assert.Equal(dto.DisplayName, user.DisplayName);
        Assert.Equal(dto.Department, user.Department);
        Assert.Equal(dto.TeamId, user.TeamId);
    }

    // Team Mappings
    [Fact]
    public void MapTeamToTeamDto_WithValidTeam_MapsCorrectly()
    {
        // Arrange
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = "Engineering",
            Description = "Software development team",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var dto = _mapper.Map<TeamDto>(team);

        // Assert
        Assert.Equal(team.Id, dto.Id);
        Assert.Equal(team.Name, dto.Name);
        Assert.Equal(team.Description, dto.Description);
    }

    [Fact]
    public void MapCreateTeamDtoToTeam_WithValidDto_MapsCorrectly()
    {
        // Arrange
        var dto = new CreateTeamDto
        {
            Name = "New Team",
            Description = "New team description"
        };

        // Act
        var team = _mapper.Map<Team>(dto);

        // Assert
        Assert.Equal(dto.Name, team.Name);
        Assert.Equal(dto.Description, team.Description);
    }

    [Fact]
    public void MapUpdateTeamDtoToTeam_WithValidDto_MapsCorrectly()
    {
        // Arrange
        var dto = new UpdateTeamDto
        {
            Name = "Updated Team",
            Description = "Updated description"
        };

        // Act
        var team = _mapper.Map<Team>(dto);

        // Assert
        Assert.Equal(dto.Name, team.Name);
        Assert.Equal(dto.Description, team.Description);
    }

    [Fact]
    public void MapVacationListToVacationDtoList_WithMultipleVacations_MapsAll()
    {
        // Arrange
        var vacations = new[]
        {
            new Vacation { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5) },
            new Vacation { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(3) },
            new Vacation { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(7) }
        };

        // Act
        var dtos = _mapper.Map<List<VacationDto>>(vacations);

        // Assert
        Assert.Equal(3, dtos.Count);
        for (int i = 0; i < vacations.Length; i++)
        {
            Assert.Equal(vacations[i].Id, dtos[i].Id);
        }
    }

    [Fact]
    public void MapUserListToUserDtoList_WithMultipleUsers_MapsAll()
    {
        // Arrange
        var users = new[]
        {
            new User { Id = Guid.NewGuid(), EntraId = "entra-1", Email = "user1@example.com", DisplayName = "User One" },
            new User { Id = Guid.NewGuid(), EntraId = "entra-2", Email = "user2@example.com", DisplayName = "User Two" },
            new User { Id = Guid.NewGuid(), EntraId = "entra-3", Email = "user3@example.com", DisplayName = "User Three" }
        };

        // Act
        var dtos = _mapper.Map<List<UserDto>>(users);

        // Assert
        Assert.Equal(3, dtos.Count);
        for (int i = 0; i < users.Length; i++)
        {
            Assert.Equal(users[i].Id, dtos[i].Id);
        }
    }

    [Fact]
    public void MapTeamListToTeamDtoList_WithMultipleTeams_MapsAll()
    {
        // Arrange
        var teams = new[]
        {
            new Team { Id = Guid.NewGuid(), Name = "Engineering" },
            new Team { Id = Guid.NewGuid(), Name = "Marketing" },
            new Team { Id = Guid.NewGuid(), Name = "Sales" }
        };

        // Act
        var dtos = _mapper.Map<List<TeamDto>>(teams);

        // Assert
        Assert.Equal(3, dtos.Count);
        for (int i = 0; i < teams.Length; i++)
        {
            Assert.Equal(teams[i].Id, dtos[i].Id);
        }
    }
}
