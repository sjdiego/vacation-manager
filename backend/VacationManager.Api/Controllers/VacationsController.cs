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
public class VacationsController : ControllerBase
{
    private readonly IVacationRepository _vacationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<VacationsController> _logger;
    private readonly IClaimExtractorService _claimExtractor;

    public VacationsController(
        IVacationRepository vacationRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<VacationsController> logger,
        IClaimExtractorService claimExtractor)
    {
        _vacationRepository = vacationRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _claimExtractor = claimExtractor;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VacationDto>>> GetMyVacations()
    {
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null)
            return NotFound("User not found");

        var vacations = await _vacationRepository.GetByUserIdAsync(user.Id);
        return Ok(_mapper.Map<List<VacationDto>>(vacations));
    }

    [HttpGet("team/pending")]
    public async Task<ActionResult<IEnumerable<VacationDto>>> GetTeamPendingVacations()
    {
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null || !user.IsManager || user.TeamId == null)
            return Forbid("Only team managers can view pending vacations");

        var vacations = await _vacationRepository.GetByTeamAsync(user.TeamId.Value);
        var pendingVacations = vacations
            .Where(v => v.Status == VacationStatus.Pending)
            .ToList();
        
        return Ok(_mapper.Map<List<VacationDto>>(pendingVacations));
    }

    [HttpGet("team")]
    public async Task<ActionResult<IEnumerable<VacationDto>>> GetTeamVacations([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null || user.TeamId == null)
            return BadRequest("User must be part of a team");

        var vacations = await _vacationRepository.GetByTeamAsync(user.TeamId.Value);
        
        if (startDate.HasValue && endDate.HasValue)
        {
            vacations = vacations
                .Where(v => v.StartDate <= endDate && v.EndDate >= startDate)
                .ToList();
        }
        
        return Ok(_mapper.Map<List<VacationDto>>(vacations));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VacationDto>> GetById(Guid id)
    {
        var vacation = await _vacationRepository.GetByIdAsync(id);
        if (vacation == null)
            return NotFound();

        return Ok(_mapper.Map<VacationDto>(vacation));
    }

    [HttpPost]
    public async Task<ActionResult<VacationDto>> Create(CreateVacationDto dto)
    {
        var userEntraId = _claimExtractor.GetEntraId(User);
        if (string.IsNullOrEmpty(userEntraId))
            return Unauthorized();

        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        if (user == null)
            return NotFound("User not found");

        if (user.TeamId == null)
            return BadRequest("User must be part of a team to request vacation");

        var userVacations = await _vacationRepository.GetByUserIdAsync(user.Id);
        var hasOverlap = userVacations.Any(v => 
            v.Status == VacationStatus.Approved && 
            v.StartDate <= dto.EndDate && 
            v.EndDate >= dto.StartDate);

        if (hasOverlap)
            return BadRequest("You have overlapping approved vacations in this date range");

        var vacation = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Type = dto.Type,
            Notes = dto.Notes,
            Status = VacationStatus.Pending
        };

        var created = await _vacationRepository.CreateAsync(vacation);
        _logger.LogInformation("Vacation created: {VacationId} for user {UserId}", created.Id, user.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, _mapper.Map<VacationDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VacationDto>> Update(Guid id, UpdateVacationDto dto)
    {
        var vacation = await _vacationRepository.GetByIdAsync(id);
        if (vacation == null)
            return NotFound();

        var userEntraId = _claimExtractor.GetEntraId(User);
        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        
        if (vacation.UserId != user?.Id)
            return Forbid();

        vacation.StartDate = dto.StartDate;
        vacation.EndDate = dto.EndDate;
        vacation.Type = dto.Type;
        vacation.Notes = dto.Notes;
        vacation.UpdatedAt = DateTime.UtcNow;

        var updated = await _vacationRepository.UpdateAsync(vacation);
        return Ok(_mapper.Map<VacationDto>(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var vacation = await _vacationRepository.GetByIdAsync(id);
        if (vacation == null)
            return NotFound();

        var userEntraId = _claimExtractor.GetEntraId(User);
        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        
        if (vacation.UserId != user?.Id)
            return Forbid();

        await _vacationRepository.DeleteAsync(id);
        _logger.LogInformation("Vacation deleted: {VacationId}", id);

        return NoContent();
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<VacationDto>> Approve(Guid id, [FromBody] ApproveVacationDto dto)
    {
        var vacation = await _vacationRepository.GetByIdAsync(id);
        if (vacation == null)
            return NotFound();

        var userEntraId = _claimExtractor.GetEntraId(User);
        var manager = await _userRepository.GetByEntraIdAsync(userEntraId);
        
        if (manager == null || !manager.IsManager)
            return Forbid("Only managers can approve vacations");

        var user = await _userRepository.GetByIdAsync(vacation.UserId);
        if (user?.TeamId != manager.TeamId)
            return Forbid("Can only approve vacations for team members");

        vacation.Status = dto.Approved ? VacationStatus.Approved : VacationStatus.Rejected;
        vacation.ApprovedBy = manager.Id;
        vacation.UpdatedAt = DateTime.UtcNow;

        var updated = await _vacationRepository.UpdateAsync(vacation);
        _logger.LogInformation("Vacation {VacationId} {Status} by {ManagerId}", 
            id, vacation.Status, manager.Id);

        return Ok(_mapper.Map<VacationDto>(updated));
    }
}
