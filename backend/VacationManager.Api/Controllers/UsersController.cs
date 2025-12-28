using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using AutoMapper;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;
using VacationManager.Api.Services;
using VacationManager.Api.Extensions;
using VacationManager.Api.Helpers;

namespace VacationManager.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;
    private readonly IClaimExtractorService _claimExtractor;
    private readonly IAuthorizationHelper _authHelper;

    public UsersController(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<UsersController> logger,
        IClaimExtractorService claimExtractor,
        IAuthorizationHelper authHelper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _claimExtractor = claimExtractor;
        _authHelper = authHelper;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return this.UnauthorizedProblem("User not authenticated");

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);

        // If user doesn't exist yet, auto-register
        if (user == null)
        {
            var email = _claimExtractor.GetEmail(User);
            var displayName = _claimExtractor.GetName(User);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(displayName))
            {
                _logger.LogWarning("Cannot auto-register user {EntraId}: missing email or display name", userEntraId);
                return this.NotFoundProblem("User not found and cannot be auto-registered");
            }

            // Check if this is the first user in the system
            var existingUsersCount = (await _userRepository.GetAllAsync()).Count();
            var isFirstUser = existingUsersCount == 0;

            user = new User
            {
                Id = Guid.NewGuid(),
                EntraId = userEntraId,
                Email = email,
                DisplayName = displayName,
                IsManager = isFirstUser,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);
            if (isFirstUser)
            {
                _logger.LogInformation("First user auto-registered as manager: {UserId} with EntraId: {EntraId}", user.Id, userEntraId);
            }
            else
            {
                _logger.LogInformation("User auto-registered: {UserId} with EntraId: {EntraId}", user.Id, userEntraId);
            }
        }

        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPost("team/{teamId}")]
    public async Task<ActionResult<UserDto>> AddToTeam(Guid teamId)
    {
        var (user, authResult) = await _authHelper.EnsureAuthenticatedAsync(User);
        if (!authResult.IsAuthorized || user == null)
            return this.UnauthorizedProblem(authResult.FailureReason ?? "Unauthorized");

        if (user.TeamId == teamId)
            return this.ConflictProblem("User is already member of this team");

        user.TeamId = teamId;
        user.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User {UserId} added to team {TeamId}", user.Id, teamId);

        return Ok(_mapper.Map<UserDto>(updated));
    }

    [HttpDelete("team")]
    public async Task<ActionResult<UserDto>> RemoveFromTeam()
    {
        var (user, authResult) = await _authHelper.EnsureAuthenticatedAsync(User);
        if (!authResult.IsAuthorized)
            return this.UnauthorizedProblem(authResult.FailureReason ?? "Unauthorized");

        if (user!.TeamId == null)
            return this.BadRequestProblem("User is not member of any team");

        var previousTeamId = user.TeamId;
        user.TeamId = null;
        user.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User {UserId} removed from team {TeamId}", user.Id, previousTeamId);

        return Ok(_mapper.Map<UserDto>(updated));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var (_, authResult) = await _authHelper.EnsureManagerAsync(User);
        if (!authResult.IsAuthorized)
            return this.ForbiddenProblem(authResult.FailureReason ?? "Unauthorized");

        var users = await _userRepository.GetAllAsync();
        return Ok(_mapper.Map<List<UserDto>>(users));
    }

    [HttpPut("{id}/team/{teamId}")]
    public async Task<ActionResult<UserDto>> AssignUserToTeam(Guid id, Guid teamId)
    {
        var (manager, authResult) = await _authHelper.EnsureManagerAsync(User);
        if (!authResult.IsAuthorized)
            return this.ForbiddenProblem(authResult.FailureReason ?? "Unauthorized");

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound("User not found");

        if (user.TeamId == teamId)
            return this.ConflictProblem("User is already member of this team");

        user.TeamId = teamId;
        user.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User {UserId} assigned to team {TeamId} by manager {ManagerId}", user.Id, teamId, manager!.Id);

        return Ok(_mapper.Map<UserDto>(updated));
    }

    [HttpDelete("{id}/team")]
    public async Task<ActionResult<UserDto>> RemoveUserFromTeam(Guid id)
    {
        var (manager, authResult) = await _authHelper.EnsureManagerAsync(User);
        if (!authResult.IsAuthorized)
            return this.ForbiddenProblem(authResult.FailureReason ?? "Unauthorized");

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound("User not found");

        if (user.TeamId == null)
            return this.BadRequestProblem("User is not member of any team");

        var previousTeamId = user.TeamId;
        user.TeamId = null;
        user.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User {UserId} removed from team {TeamId} by manager {ManagerId}", user.Id, previousTeamId, manager!.Id);

        return Ok(_mapper.Map<UserDto>(updated));
    }

     [HttpGet("team/{teamId}")]
     public async Task<ActionResult<IEnumerable<UserDto>>> GetByTeam(Guid teamId)
     {
         var (_, authResult) = await _authHelper.EnsureTeamMemberOrManagerAsync(User, teamId);
         if (!authResult.IsAuthorized)
             return this.ForbiddenProblem(authResult.FailureReason ?? "Forbidden");

         var users = await _userRepository.GetByTeamAsync(teamId);
         return Ok(_mapper.Map<List<UserDto>>(users));
     }}
