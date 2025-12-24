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
public class TeamsController : ControllerBase
{
    private readonly ITeamRepository _teamRepository;
    private readonly IVacationRepository _vacationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TeamsController> _logger;
    private readonly IClaimExtractorService _claimExtractor;

    public TeamsController(
        ITeamRepository teamRepository,
        IVacationRepository vacationRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<TeamsController> logger,
        IClaimExtractorService claimExtractor)
    {
        _teamRepository = teamRepository;
        _vacationRepository = vacationRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _claimExtractor = claimExtractor;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetAll()
    {
        var teams = await _teamRepository.GetAllAsync();
        return Ok(_mapper.Map<List<TeamDto>>(teams));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TeamDto>> GetById(Guid id)
    {
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
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null || !user.IsManager)
            return BadRequest(new { error = "Only managers can create teams" });

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        var created = await _teamRepository.CreateAsync(team);
        _logger.LogInformation("Team created: {TeamId} by manager {UserId}", created.Id, user.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, _mapper.Map<TeamDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TeamDto>> Update(Guid id, UpdateTeamDto dto)
    {
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null || !user.IsManager)
            return BadRequest(new { error = "Only managers can update teams" });

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
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null || !user.IsManager)
            return BadRequest(new { error = "Only managers can delete teams" });

        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
            return NotFound();

        await _teamRepository.DeleteAsync(id);
        _logger.LogInformation("Team deleted: {TeamId} by manager {UserId}", id, user.Id);

        return NoContent();
    }
}
