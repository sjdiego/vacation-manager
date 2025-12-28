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
public class TeamsController : ControllerBase
{
    private readonly ITeamRepository _teamRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TeamsController> _logger;
    private readonly IAuthorizationHelper _authHelper;

    public TeamsController(
        ITeamRepository teamRepository,
        IVacationRepository vacationRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<TeamsController> logger,
        IAuthorizationHelper authHelper)
    {
        _teamRepository = teamRepository;
        _vacationRepository = vacationRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _authHelper = authHelper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetAll()
    {
        var user = await _authHelper.GetCurrentUserAsync(User);
        if (user == null)
            return Unauthorized();

        // Managers can see all teams, regular users only see their teams
        IEnumerable<Team> teams = user.IsManager
            ? await _teamRepository.GetAllAsync()
            : await _teamRepository.GetByUserAsync(user.Id);

        return Ok(_mapper.Map<List<TeamDto>>(teams));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TeamDto>> GetById(Guid id)
    {
        var (_, authResult) = await _authHelper.EnsureTeamMemberOrManagerAsync(User, id);
        if (!authResult.IsAuthorized)
            return this.ForbiddenProblem(authResult.FailureReason ?? "Forbidden");

        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
            return NotFound();

        return Ok(_mapper.Map<TeamDto>(team));
    }

    [HttpGet("{id}/vacations")]
    public async Task<ActionResult<IEnumerable<VacationDto>>> GetTeamVacations(Guid id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
            return NotFound();

        IEnumerable<Vacation> vacations;
        
        if (startDate.HasValue && endDate.HasValue)
        {
            vacations = await _vacationRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
            vacations = vacations.Where(v => v.User?.TeamId == id);
        }
        else
        {
            vacations = await _vacationRepository.GetByTeamAsync(id);
        }

        return Ok(_mapper.Map<List<VacationDto>>(vacations));
    }

    [HttpPost]
    public async Task<ActionResult<TeamDto>> Create(CreateTeamDto dto)
    {
        var (manager, authResult) = await _authHelper.EnsureManagerAsync(User);
        if (!authResult.IsAuthorized)
            return this.ForbiddenProblem(authResult.FailureReason ?? "Forbidden");

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        var created = await _teamRepository.CreateAsync(team);
        _logger.LogInformation("Team created: {TeamId} by manager {UserId}", created.Id, manager!.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, _mapper.Map<TeamDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TeamDto>> Update(Guid id, UpdateTeamDto dto)
    {
        var (_, authResult) = await _authHelper.EnsureManagerAsync(User);
        if (!authResult.IsAuthorized)
            return this.ForbiddenProblem(authResult.FailureReason ?? "Forbidden");

        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.Name))
            team.Name = dto.Name;
        if (dto.Description != null)
            team.Description = dto.Description;

        team.UpdatedAt = DateTime.UtcNow;

        var updated = await _teamRepository.UpdateAsync(team);
        return Ok(_mapper.Map<TeamDto>(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (manager, authResult) = await _authHelper.EnsureManagerAsync(User);
        if (!authResult.IsAuthorized)
            return this.ForbiddenProblem(authResult.FailureReason ?? "Forbidden");

        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
            return NotFound();

        await _teamRepository.DeleteAsync(id);
        _logger.LogInformation("Team deleted: {TeamId} by manager {UserId}", id, manager!.Id);

        return NoContent();
    }
}
