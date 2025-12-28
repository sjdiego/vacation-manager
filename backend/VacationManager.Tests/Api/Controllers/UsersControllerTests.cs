using Xunit;
using NSubstitute;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VacationManager.Api.Controllers;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;
using VacationManager.Api.Services;
using VacationManager.Api.Helpers;

namespace VacationManager.Tests.Api.Controllers;

public class UsersControllerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;
    private readonly IClaimExtractorService _claimExtractor;
    private readonly IAuthorizationHelper _authHelper;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<UsersController>>();
        _claimExtractor = Substitute.For<IClaimExtractorService>();
        _authHelper = Substitute.For<IAuthorizationHelper>();
        _controller = new UsersController(_userRepository, _mapper, _logger, _claimExtractor, _authHelper);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, EntraId = "entra-id", Email = "user@example.com", DisplayName = "John Doe" };
        var userDto = new UserDto { Id = userId };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<UserDto>(okResult.Value);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_WithNonManager_ReturnsForbidden()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", IsManager = false };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((user, VacationManager.Core.Authorization.AuthorizationResult.Failure("User is not a manager")));

        // Act
        var result = await _controller.GetAll();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithManager_ReturnsAllUsers()
    {
        // Arrange
        var manager = new User { Id = Guid.NewGuid(), EntraId = "manager-entra-id", Email = "manager@example.com", DisplayName = "Manager", IsManager = true };
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), EntraId = "entra-1", Email = "user1@example.com", DisplayName = "User 1" },
            new User { Id = Guid.NewGuid(), EntraId = "entra-2", Email = "user2@example.com", DisplayName = "User 2" }
        };
        var userDtos = new List<UserDto> { new UserDto(), new UserDto() };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((manager, VacationManager.Core.Authorization.AuthorizationResult.Success()));
        _userRepository.GetAllAsync().Returns(users);
        _mapper.Map<List<UserDto>>(users).Returns(userDtos);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDtos = Assert.IsType<List<UserDto>>(okResult.Value);
        Assert.Equal(2, returnedDtos.Count);
    }

    [Fact]
    public async Task GetMe_WithValidUser_ReturnsCurrentUser()
    {
        // Arrange
        var entraId = "user-entra-id";
        var user = new User { Id = Guid.NewGuid(), EntraId = entraId, Email = "user@example.com", DisplayName = "John Doe" };
        var userDto = new UserDto { Id = user.Id };

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns(user);
        _mapper.Map<UserDto>(user).Returns(userDto);

        // Act
        var result = await _controller.GetMe();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<UserDto>(okResult.Value);
    }

    [Fact]
    public async Task GetMe_WithNewUser_AutoRegistersAndReturnsUser()
    {
        // Arrange
        var entraId = "new-user-entra-id";
        var email = "newuser@example.com";
        var displayName = "New User";
        var createdUser = new User { Id = Guid.NewGuid(), EntraId = entraId, Email = email, DisplayName = displayName };
        var userDto = new UserDto { Id = createdUser.Id };

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns((User?)null);
        _userRepository.GetAllAsync().Returns(new List<User> { new User { Id = Guid.NewGuid(), EntraId = "other", Email = "other@example.com", DisplayName = "Other" } }); // Not first user
        _claimExtractor.GetEmail(_controller.User).Returns(email);
        _claimExtractor.GetName(_controller.User).Returns(displayName);
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(createdUser);
        _mapper.Map<UserDto>(createdUser).Returns(userDto);

        // Act
        var result = await _controller.GetMe();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<UserDto>(okResult.Value);
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task GetMe_WithFirstUser_AutoRegistersAsManagerAndReturnsUser()
    {
        // Arrange
        var entraId = "first-user-entra-id";
        var email = "firstuser@example.com";
        var displayName = "First User";
        var createdUser = new User { Id = Guid.NewGuid(), EntraId = entraId, Email = email, DisplayName = displayName, IsManager = true };
        var userDto = new UserDto { Id = createdUser.Id, IsManager = true };

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns((User?)null);
        _userRepository.GetAllAsync().Returns(new List<User>()); // First user - empty list
        _claimExtractor.GetEmail(_controller.User).Returns(email);
        _claimExtractor.GetName(_controller.User).Returns(displayName);
        _userRepository.CreateAsync(Arg.Is<User>(u => u.IsManager)).Returns(createdUser);
        _mapper.Map<UserDto>(createdUser).Returns(userDto);

        // Act
        var result = await _controller.GetMe();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<UserDto>(okResult.Value);
        await _userRepository.Received(1).CreateAsync(Arg.Is<User>(u => u.IsManager));
    }

    [Fact]
    public async Task GetMe_WithNewUserMissingClaims_ReturnsNotFound()
    {
        // Arrange
        var entraId = "new-user-entra-id";

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns((User?)null);
        _claimExtractor.GetEmail(_controller.User).Returns(string.Empty);
        _claimExtractor.GetName(_controller.User).Returns(string.Empty);

        // Act
        var result = await _controller.GetMe();

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task AddToTeam_WithValidTeam_AddsUserToTeam()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var entraId = "user-entra-id";
        var user = new User { Id = userId, EntraId = entraId, Email = "user@example.com", DisplayName = "User", TeamId = null };
        var updatedUser = new User { Id = userId, EntraId = entraId, Email = "user@example.com", DisplayName = "User", TeamId = teamId };
        var userDto = new UserDto { Id = userId, TeamId = teamId };

        _authHelper.EnsureAuthenticatedAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((user, VacationManager.Core.Authorization.AuthorizationResult.Success()));
        _userRepository.UpdateAsync(Arg.Any<User>()).Returns(updatedUser);
        _mapper.Map<UserDto>(updatedUser).Returns(userDto);

        // Act
        var result = await _controller.AddToTeam(teamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        await _userRepository.Received(1).UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task GetByTeam_WithUserNotInTeam_ReturnsForbidden()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", TeamId = null, IsManager = false };

        _authHelper.EnsureTeamMemberOrManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>(), teamId)
            .Returns((user, VacationManager.Core.Authorization.AuthorizationResult.Failure("User is not in the team")));

        // Act
        var result = await _controller.GetByTeam(teamId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetByTeam_WithValidTeamId_ReturnsTeamMembers()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", TeamId = teamId, IsManager = false };
        var users = new List<User> { new User { Id = Guid.NewGuid(), EntraId = "entra-1", Email = "user1@example.com", DisplayName = "User 1", TeamId = teamId } };
        var userDtos = new List<UserDto> { new UserDto() };

        _authHelper.EnsureTeamMemberOrManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>(), teamId)
            .Returns((user, VacationManager.Core.Authorization.AuthorizationResult.Success()));
        _userRepository.GetByTeamAsync(teamId).Returns(users);
        _mapper.Map<List<UserDto>>(users).Returns(userDtos);

        // Act
        var result = await _controller.GetByTeam(teamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task AssignUserToTeam_WithNonManager_ReturnsForbidden()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", IsManager = false };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((user, VacationManager.Core.Authorization.AuthorizationResult.Failure("User is not a manager")));

        // Act
        var result = await _controller.AssignUserToTeam(userId, teamId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task AssignUserToTeam_WithManager_AssignsUserToTeam()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var manager = new User { Id = Guid.NewGuid(), EntraId = "manager-entra-id", Email = "manager@example.com", DisplayName = "Manager", IsManager = true };
        var user = new User { Id = userId, EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", TeamId = null };
        var updatedUser = new User { Id = userId, EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", TeamId = teamId };
        var userDto = new UserDto { Id = userId, TeamId = teamId };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((manager, VacationManager.Core.Authorization.AuthorizationResult.Success()));
        _userRepository.GetByIdAsync(userId).Returns(user);
        _userRepository.UpdateAsync(Arg.Any<User>()).Returns(updatedUser);
        _mapper.Map<UserDto>(updatedUser).Returns(userDto);

        // Act
        var result = await _controller.AssignUserToTeam(userId, teamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        await _userRepository.Received(1).UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task RemoveUserFromTeam_WithNonManager_ReturnsForbidden()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", IsManager = false };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((user, VacationManager.Core.Authorization.AuthorizationResult.Failure("User is not a manager")));

        // Act
        var result = await _controller.RemoveUserFromTeam(userId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task RemoveUserFromTeam_WithManager_RemovesUserFromTeam()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var manager = new User { Id = Guid.NewGuid(), EntraId = "manager-entra-id", Email = "manager@example.com", DisplayName = "Manager", IsManager = true };
        var user = new User { Id = userId, EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", TeamId = teamId };
        var updatedUser = new User { Id = userId, EntraId = "user-entra-id", Email = "user@example.com", DisplayName = "User", TeamId = null };
        var userDto = new UserDto { Id = userId };

        _authHelper.EnsureManagerAsync(Arg.Any<System.Security.Claims.ClaimsPrincipal>())
            .Returns((manager, VacationManager.Core.Authorization.AuthorizationResult.Success()));
        _userRepository.GetByIdAsync(userId).Returns(user);
        _userRepository.UpdateAsync(Arg.Any<User>()).Returns(updatedUser);
        _mapper.Map<UserDto>(updatedUser).Returns(userDto);

        // Act
        var result = await _controller.RemoveUserFromTeam(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        await _userRepository.Received(1).UpdateAsync(Arg.Any<User>());
    }
}
