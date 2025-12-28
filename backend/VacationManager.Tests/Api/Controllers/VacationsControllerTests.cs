using Xunit;
using NSubstitute;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VacationManager.Api.Controllers;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;
using VacationManager.Core.Validation;
using VacationManager.Api.Services;

namespace VacationManager.Tests.Api.Controllers;

public class VacationsControllerTests
{
    private readonly IVacationRepository _vacationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<VacationsController> _logger;
    private readonly IClaimExtractorService _claimExtractor;
    private readonly IVacationValidationService _validationService;
    private readonly VacationsController _controller;

    public VacationsControllerTests()
    {
        _vacationRepository = Substitute.For<IVacationRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<VacationsController>>();
        _claimExtractor = Substitute.For<IClaimExtractorService>();
        _validationService = Substitute.For<IVacationValidationService>();
        _controller = new VacationsController(
            _vacationRepository,
            _userRepository,
            _mapper,
            _logger,
            _claimExtractor,
            _validationService);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsVacation()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var vacation = new Vacation { Id = vacationId, UserId = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5) };
        var vacationDto = new VacationDto { Id = vacationId };

        _vacationRepository.GetByIdAsync(vacationId).Returns(vacation);
        _mapper.Map<VacationDto>(vacation).Returns(vacationDto);

        // Act
        var result = await _controller.GetById(vacationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<VacationDto>(okResult.Value);
        Assert.Equal(vacationId, returnedDto.Id);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        _vacationRepository.GetByIdAsync(vacationId).Returns((Vacation?)null);

        // Act
        var result = await _controller.GetById(vacationId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetMyVacations_WithValidUser_ReturnsVacations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entraId = "user-entra-id";
        var user = new User { Id = userId, EntraId = entraId, Email = "user@example.com", DisplayName = "John Doe" };
        var vacations = new List<Vacation> { new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5) } };
        var vacationDtos = new List<VacationDto> { new VacationDto { Id = vacations[0].Id } };

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns(user);
        _vacationRepository.GetByUserIdAsync(userId).Returns(vacations);
        _mapper.Map<List<VacationDto>>(vacations).Returns(vacationDtos);

        // Act
        var result = await _controller.GetMyVacations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDtos = Assert.IsType<List<VacationDto>>(okResult.Value);
        Assert.Single(returnedDtos);
    }

    [Fact]
    public async Task GetMyVacations_WithNullEntraId_ReturnsUnauthorized()
    {
        // Arrange
        _claimExtractor.GetEntraId(_controller.User).Returns((string?)null);

        // Act
        var result = await _controller.GetMyVacations();

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithUserNotInTeam_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entraId = "user-entra-id";
        var user = new User { Id = userId, EntraId = entraId, Email = "user@example.com", DisplayName = "John Doe", TeamId = null };
        var createDto = new CreateVacationDto { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), Type = VacationType.Vacation };

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns(user);
        _validationService.ValidateAsync(Arg.Any<Vacation>(), Arg.Any<User>())
            .Returns(VacationManager.Core.Validation.ValidationResult.Failure("User must be part of a team to request vacation", "TEAM_MEMBERSHIP_REQUIRED"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        await _vacationRepository.DidNotReceive().CreateAsync(Arg.Any<Vacation>());
    }

    [Fact]
    public async Task Create_WithValidDataAndTeam_CreatesVacation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var entraId = "user-entra-id";
        var user = new User { Id = userId, EntraId = entraId, Email = "user@example.com", DisplayName = "John Doe", TeamId = teamId };
        var createDto = new CreateVacationDto { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), Type = VacationType.Vacation };
        var createdVacation = new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = createDto.StartDate, EndDate = createDto.EndDate, Type = createDto.Type, Status = VacationStatus.Pending };
        var vacationDto = new VacationDto { Id = createdVacation.Id };

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns(user);
        _vacationRepository.GetByUserIdAsync(userId).Returns(new List<Vacation>());
        _vacationRepository.CreateAsync(Arg.Any<Vacation>()).Returns(createdVacation);
        _mapper.Map<VacationDto>(createdVacation).Returns(vacationDto);
        _validationService.ValidateAsync(Arg.Any<Vacation>(), Arg.Any<User>())
            .Returns(VacationManager.Core.Validation.ValidationResult.Success());

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(VacationsController.GetById), createdResult.ActionName);
        await _vacationRepository.Received(1).CreateAsync(Arg.Any<Vacation>());
    }

    [Fact]
    public async Task Create_WithOverlappingVacations_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entraId = "user-entra-id";
        var user = new User { Id = userId, EntraId = entraId, Email = "user@example.com", DisplayName = "John Doe" };
        var existingVacation = new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(10), Status = VacationStatus.Approved };
        var createDto = new CreateVacationDto { StartDate = DateTime.UtcNow.AddDays(3), EndDate = DateTime.UtcNow.AddDays(6) };

        _claimExtractor.GetEntraId(_controller.User).Returns(entraId);
        _userRepository.GetByEntraIdAsync(entraId).Returns(user);
        _vacationRepository.GetByUserIdAsync(userId).Returns(new List<Vacation> { existingVacation });
        _validationService.ValidateAsync(Arg.Any<Vacation>(), Arg.Any<User>())
            .Returns(VacationManager.Core.Validation.ValidationResult.Failure("You have overlapping approved vacations in this date range", "VACATION_OVERLAP"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        Assert.IsType<ConflictObjectResult>(result.Result);
        await _vacationRepository.DidNotReceive().CreateAsync(Arg.Any<Vacation>());
    }

    [Fact]
    public async Task Approve_WithManagerApproving_ApprovesVacation()
    {
        // Arrange
        var vacationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var managerEntraId = "manager-entra-id";

        var vacation = new Vacation { Id = vacationId, UserId = userId, Status = VacationStatus.Pending };
        var user = new User { Id = userId, TeamId = teamId, EntraId = "user-entra", Email = "user@example.com", DisplayName = "User" };
        var manager = new User { Id = managerId, EntraId = managerEntraId, Email = "manager@example.com", DisplayName = "Manager", IsManager = true, TeamId = teamId };
        var approveDto = new ApproveVacationDto { Approved = true };
        var updatedVacation = new Vacation { Id = vacationId, Status = VacationStatus.Approved };
        var vacationDto = new VacationDto { Id = vacationId, Status = VacationStatus.Approved };

        _vacationRepository.GetByIdAsync(vacationId).Returns(vacation);
        _claimExtractor.GetEntraId(_controller.User).Returns(managerEntraId);
        _userRepository.GetByEntraIdAsync(managerEntraId).Returns(manager);
        _userRepository.GetByIdAsync(userId).Returns(user);
        _vacationRepository.UpdateAsync(Arg.Any<Vacation>()).Returns(updatedVacation);
        _mapper.Map<VacationDto>(updatedVacation).Returns(vacationDto);

        // Act
        var result = await _controller.Approve(vacationId, approveDto);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        await _vacationRepository.Received(1).UpdateAsync(Arg.Any<Vacation>());
    }

    [Fact]
    public async Task GetTeamPendingVacations_WithManager_ReturnsPendingVacations()
    {
        // Arrange
        var managerEntraId = "manager-entra-id";
        var managerId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var manager = new User { Id = managerId, EntraId = managerEntraId, Email = "manager@example.com", DisplayName = "Manager", IsManager = true, TeamId = teamId };
        var vacations = new List<Vacation> { new Vacation { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(5), Status = VacationStatus.Pending } };
        var vacationDtos = new List<VacationDto> { new VacationDto() };

        _claimExtractor.GetEntraId(_controller.User).Returns(managerEntraId);
        _userRepository.GetByEntraIdAsync(managerEntraId).Returns(manager);
        _vacationRepository.GetByTeamAsync(teamId).Returns(vacations);
        _mapper.Map<List<VacationDto>>(Arg.Any<List<Vacation>>()).Returns(vacationDtos);

        // Act
        var result = await _controller.GetTeamPendingVacations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }
}
