using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;
using VacationManager.Api.Services;

namespace VacationManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;
    private readonly IClaimExtractorService _claimExtractor;

    public UsersController(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<UsersController> logger,
        IClaimExtractorService claimExtractor)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _claimExtractor = claimExtractor;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null)
        {
            var email = _claimExtractor.GetEmail(User);
            var displayName = _claimExtractor.GetName(User);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(displayName))
            {
                _logger.LogWarning("Cannot auto-register user {EntraId}: missing email or display name", userEntraId);
                return NotFound();
            }

            user = new User
            {
                Id = Guid.NewGuid(),
                EntraId = userEntraId,
                Email = email,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);
            _logger.LogInformation("User auto-registered: {UserId} with EntraId: {EntraId} and email: {Email}", user.Id, userEntraId, email);
        }

        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpPost("team/{teamId}")]
    public async Task<ActionResult<UserDto>> AddToTeam(Guid teamId)
    {
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null)
            return NotFound("User not found");

        if (user.TeamId == teamId)
            return BadRequest(new { error = "User is already member of this team" });

        user.TeamId = teamId;
        user.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User {UserId} added to team {TeamId}", user.Id, teamId);

        return Ok(_mapper.Map<UserDto>(updated));
    }

    [HttpDelete("team")]
    public async Task<ActionResult<UserDto>> RemoveFromTeam()
    {
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null)
            return NotFound("User not found");

        if (user.TeamId == null)
            return BadRequest(new { error = "User is not member of any team" });

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
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null || !user.IsManager)
            return Forbid("Only managers can view all users");

        var users = await _userRepository.GetAllAsync();
        return Ok(_mapper.Map<List<UserDto>>(users));
    }

    [HttpPut("{id}/team/{teamId}")]
    public async Task<ActionResult<UserDto>> AssignUserToTeam(Guid id, Guid teamId)
    {
        var managerEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(managerEntraId))
            return Unauthorized();

        var manager = await _userRepository.GetByEntraIdAsync(managerEntraId);
        if (manager == null || !manager.IsManager)
            return Forbid("Only managers can assign users to teams");

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound("User not found");

        if (user.TeamId == teamId)
            return BadRequest(new { error = "User is already member of this team" });

        user.TeamId = teamId;
        user.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User {UserId} assigned to team {TeamId} by manager {ManagerId}", user.Id, teamId, manager.Id);

        return Ok(_mapper.Map<UserDto>(updated));
    }

    [HttpDelete("{id}/team")]
    public async Task<ActionResult<UserDto>> RemoveUserFromTeam(Guid id)
    {
        var managerEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(managerEntraId))
            return Unauthorized();

        var manager = await _userRepository.GetByEntraIdAsync(managerEntraId);
        if (manager == null || !manager.IsManager)
            return Forbid("Only managers can remove users from teams");

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound("User not found");

        if (user.TeamId == null)
            return BadRequest(new { error = "User is not member of any team" });

        var previousTeamId = user.TeamId;
        user.TeamId = null;
        user.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User {UserId} removed from team {TeamId} by manager {ManagerId}", user.Id, previousTeamId, manager.Id);

        return Ok(_mapper.Map<UserDto>(updated));
    }

     [HttpGet("team/{teamId}")]
     public async Task<ActionResult<IEnumerable<UserDto>>> GetByTeam(Guid teamId)
     {
         var userEntraId = _claimExtractor.GetEntraId(User);
         if (string.IsNullOrEmpty(userEntraId))
             return Unauthorized();

         var user = await _userRepository.GetByEntraIdAsync(userEntraId);
         if (user == null || user.TeamId == null)
             return Forbid("User must be part of a team");

         if (user.TeamId != teamId && !user.IsManager)
             return Forbid("Can only view users from your own team");

         var users = await _userRepository.GetByTeamAsync(teamId);
         return Ok(_mapper.Map<List<UserDto>>(users));
     }}