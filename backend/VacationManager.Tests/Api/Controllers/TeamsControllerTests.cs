using Xunit;
using NSubstitute;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VacationManager.Api.Controllers;
using VacationManager.Api.Services;
using VacationManager.Api.Helpers;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;

namespace VacationManager.Tests.Api.Controllers;

public class TeamsControllerTests
{
    private readonly ITeamRepository _teamRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TeamsController> _logger;
    private readonly IAuthorizationHelper _authHelper;
    private readonly TeamsController _controller;

    public TeamsControllerTests()
    {
        _teamRepository = Substitute.For<ITeamRepository>();
        _vacationRepository = Substitute.For<IVacationRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<TeamsController>>();
        _authHelper = Substitute.For<IAuthorizationHelper>();
        _controller = new TeamsController(_teamRepository, _vacationRepository, _userRepository, _mapper, _logger, _authHelper);
    }

    [Fact]
    public async Task GetAll_ReturnsAllTeams()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraId = "user123", Email = "user@test.com", DisplayName = "Test User", IsManager = true };
        var teams = new List<Team> { new Team { Id = Guid.NewGuid(), Name = "Engineering" }, new Team { Id = Guid.NewGuid(), Name = "Marketing" } };
        var teamDtos = new List<TeamDto> { new TeamDto(), new TeamDto() };

        _authHelper.GetCurrentUserAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);
        _teamRepository.GetAllAsync().Returns(teams);
        _mapper.Map<List<TeamDto>>(teams).Returns(teamDtos);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDtos = Assert.IsType<List<TeamDto>>(okResult.Value);
        Assert.Equal(2, returnedDtos.Count);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsTeam()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var team = new Team { Id = teamId, Name = "Engineering" };
        var teamDto = new TeamDto { Id = teamId, Name = "Engineering" };

        _teamRepository.GetByIdAsync(teamId).Returns(team);
        _mapper.Map<TeamDto>(team).Returns(teamDto);

        // Act
        var result = await _controller.GetById(teamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<TeamDto>(okResult.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        _teamRepository.GetByIdAsync(teamId).Returns((Team?)null);

        // Act
        var result = await _controller.GetById(teamId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithNonManager_ReturnsBadRequest()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", IsManager = false };
        var createDto = new CreateTeamDto { Name = "Engineering", Description = "Software team" };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((user, VacationManager.Core.Authorization.AuthorizationResult.Failure("User is not a manager")));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
    }

    [Fact]
    public async Task Create_WithManager_CreatesTeam()
    {
        // Arrange
        var manager = new User { Id = Guid.NewGuid(), EntraId = "manager-entra-id", Email = "manager@example.com", DisplayName = "Manager", IsManager = true };
        var createDto = new CreateTeamDto { Name = "Engineering", Description = "Software team" };
        var createdTeam = new Team { Id = Guid.NewGuid(), Name = createDto.Name, Description = createDto.Description };
        var teamDto = new TeamDto { Id = createdTeam.Id, Name = createdTeam.Name };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((manager, VacationManager.Core.Authorization.AuthorizationResult.Success()));
        _teamRepository.CreateAsync(Arg.Any<Team>()).Returns(createdTeam);
        _mapper.Map<TeamDto>(createdTeam).Returns(teamDto);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TeamsController.GetById), createdResult.ActionName);
        await _teamRepository.Received(1).CreateAsync(Arg.Any<Team>());
    }

    [Fact]
    public async Task Update_WithNonManager_ReturnsBadRequest()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", IsManager = false };
        var updateDto = new UpdateTeamDto { Name = "Updated Engineering", Description = "New" };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((user, VacationManager.Core.Authorization.AuthorizationResult.Failure("User is not a manager")));

        // Act
        var result = await _controller.Update(teamId, updateDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
    }

    [Fact]
    public async Task Update_WithManager_UpdatesTeam()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var manager = new User { Id = Guid.NewGuid(), EntraId = "manager-entra-id", Email = "manager@example.com", DisplayName = "Manager", IsManager = true };
        var team = new Team { Id = teamId, Name = "Engineering", Description = "Old" };
        var updateDto = new UpdateTeamDto { Name = "Updated Engineering", Description = "New" };
        var updatedTeam = new Team { Id = teamId, Name = updateDto.Name, Description = updateDto.Description };
        var teamDto = new TeamDto { Id = teamId, Name = updatedTeam.Name };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((manager, VacationManager.Core.Authorization.AuthorizationResult.Success()));
        _teamRepository.GetByIdAsync(teamId).Returns(team);
        _teamRepository.UpdateAsync(Arg.Any<Team>()).Returns(updatedTeam);
        _mapper.Map<TeamDto>(updatedTeam).Returns(teamDto);

        // Act
        var result = await _controller.Update(teamId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        await _teamRepository.Received(1).UpdateAsync(Arg.Any<Team>());
    }

    [Fact]
    public async Task Delete_WithNonManager_ReturnsBadRequest()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", IsManager = false };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((user, VacationManager.Core.Authorization.AuthorizationResult.Failure("User is not a manager")));

        // Act
        var result = await _controller.Delete(teamId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);
    }

    [Fact]
    public async Task Delete_WithManager_DeletesTeam()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var manager = new User { Id = Guid.NewGuid(), EntraId = "manager-entra-id", Email = "manager@example.com", DisplayName = "Manager", IsManager = true };
        var team = new Team { Id = teamId, Name = "Engineering" };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((manager, VacationManager.Core.Authorization.AuthorizationResult.Success()));
        _teamRepository.GetByIdAsync(teamId).Returns(team);

        // Act
        var result = await _controller.Delete(teamId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        await _teamRepository.Received(1).DeleteAsync(teamId);
    }

    [Fact]
    public async Task GetTeamVacations_WithoutDateRange_ReturnsTeamVacations()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var team = new Team { Id = teamId, Name = "Engineering" };
        var vacations = new List<Vacation> { new Vacation { Id = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5) } };
        var vacationDtos = new List<VacationDto> { new VacationDto() };

        _teamRepository.GetByIdAsync(teamId).Returns(team);
        _vacationRepository.GetByTeamAsync(teamId).Returns(vacations);
        _mapper.Map<List<VacationDto>>(vacations).Returns(vacationDtos);

        // Act
        var result = await _controller.GetTeamVacations(teamId, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetTeamVacations_WithInvalidTeamId_ReturnsNotFound()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        _teamRepository.GetByIdAsync(teamId).Returns((Team?)null);

        // Act
        var result = await _controller.GetTeamVacations(teamId, null, null);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
