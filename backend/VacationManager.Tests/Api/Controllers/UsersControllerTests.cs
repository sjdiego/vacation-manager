using Xunit;
using NSubstitute;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VacationManager.Api.Controllers;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;
using VacationManager.Api.Services;

namespace VacationManager.Tests.Api.Controllers;

public class UsersControllerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;
    private readonly IClaimExtractorService _claimExtractor;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<UsersController>>();
        _claimExtractor = Substitute.For<IClaimExtractorService>();
        _controller = new UsersController(_userRepository, _mapper, _logger, _claimExtractor);
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
    public async Task GetAll_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), EntraId = "entra-1", Email = "user1@example.com", DisplayName = "User 1" },
            new User { Id = Guid.NewGuid(), EntraId = "entra-2", Email = "user2@example.com", DisplayName = "User 2" }
        };
        var userDtos = new List<UserDto> { new UserDto(), new UserDto() };

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
    public async Task Register_WithNewUser_CreatesUser()
    {
        // Arrange
        var entraId = "new-user-entra-id";
        var email = "newuser@example.com";
        var displayName = "New User";
        var createdUser = new User { Id = Guid.NewGuid(), EntraId = entraId, Email = email, DisplayName = displayName };
        var userDto = new UserDto { Id = createdUser.Id };

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns((User?)null);
        _claimExtractor.GetEmail(_controller.User).Returns(email);
        _claimExtractor.GetName(_controller.User).Returns(displayName);
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(createdUser);
        _mapper.Map<UserDto>(createdUser).Returns(userDto);

        // Act
        var result = await _controller.Register();

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(UsersController.GetById), createdResult.ActionName);
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>());
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

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns(user);
        _userRepository.UpdateAsync(Arg.Any<User>()).Returns(updatedUser);
        _mapper.Map<UserDto>(updatedUser).Returns(userDto);

        // Act
        var result = await _controller.AddToTeam(teamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        await _userRepository.Received(1).UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task GetByTeam_WithValidTeamId_ReturnsTeamMembers()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var users = new List<User> { new User { Id = Guid.NewGuid(), EntraId = "entra-1", Email = "user1@example.com", DisplayName = "User 1", TeamId = teamId } };
        var userDtos = new List<UserDto> { new UserDto() };

        _userRepository.GetByTeamAsync(teamId).Returns(users);
        _mapper.Map<List<UserDto>>(users).Returns(userDtos);

        // Act
        var result = await _controller.GetByTeam(teamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }
}
