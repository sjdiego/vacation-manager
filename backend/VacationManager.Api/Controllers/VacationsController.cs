using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using AutoMapper;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;
using VacationManager.Core.Validation;
using VacationManager.Core.Specifications;
using VacationManager.Api.Services;
using VacationManager.Api.Extensions;

namespace VacationManager.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class VacationsController : ControllerBase
{
    private readonly IVacationRepository _vacationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<VacationsController> _logger;
    private readonly IClaimExtractorService _claimExtractor;
    private readonly IVacationValidationService _validationService;

    public VacationsController(
        IVacationRepository vacationRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<VacationsController> logger,
        IClaimExtractorService claimExtractor,
        IVacationValidationService validationService)
    {
        _vacationRepository = vacationRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _claimExtractor = claimExtractor;
        _validationService = validationService;
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
            return this.ForbiddenProblem("Only team managers can view pending vacations");

        var vacations = await _vacationRepository.GetByTeamAsync(user.TeamId.Value);
        
        // Apply Specification Pattern
        var pendingSpec = new PendingVacationsSpecification();
        var pendingVacations = vacations.Where(pendingSpec).ToList();
        
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
            return this.BadRequestProblem("User must be part of a team");

        var vacations = await _vacationRepository.GetByTeamAsync(user.TeamId.Value);
        
        // Apply Specification Pattern for date filtering
        if (startDate.HasValue && endDate.HasValue)
        {
            var dateRangeSpec = new DateRangeSpecification(startDate.Value, endDate.Value);
            vacations = vacations.Where(dateRangeSpec).ToList();
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

        var validationResult = await _validationService.ValidateAsync(vacation, user);
        if (!validationResult.IsValid)
        {
            return validationResult.ErrorCode switch
            {
                "TEAM_MEMBERSHIP_REQUIRED" => this.BadRequestProblem(validationResult.ErrorMessage!),
                "VACATION_OVERLAP" => this.ConflictProblem(validationResult.ErrorMessage!),
                _ => this.BadRequestProblem(validationResult.ErrorMessage!)
            };
        }

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
            return this.ForbiddenProblem("Only managers can approve vacations");

        var user = await _userRepository.GetByIdAsync(vacation.UserId);
        if (user?.TeamId != manager.TeamId)
            return this.ForbiddenProblem("Only vacations for team members can be approved");

        vacation.Status = dto.Approved ? VacationStatus.Approved : VacationStatus.Rejected;
        vacation.ApprovedBy = manager.Id;
        vacation.UpdatedAt = DateTime.UtcNow;

        var updated = await _vacationRepository.UpdateAsync(vacation);
        _logger.LogInformation("Vacation {VacationId} {Status} by {ManagerId}",
            id, vacation.Status, manager.Id);

        return Ok(_mapper.Map<VacationDto>(updated));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<VacationDto>> Cancel(Guid id)
    {
        var vacation = await _vacationRepository.GetByIdAsync(id);
        if (vacation == null)
            return NotFound();

        var userEntraId = _claimExtractor.GetEntraId(User);
        var user = await _userRepository.GetByEntraIdAsync(userEntraId);
        
        if (user == null)
            return Unauthorized();

        var isOwner = vacation.UserId == user.Id;
        var isManager = user.IsManager && (await _userRepository.GetByIdAsync(vacation.UserId))?.TeamId == user.TeamId;

        if (!isOwner && !isManager)
            return Forbid();

        if (vacation.Status != VacationStatus.Approved)
            return this.BadRequestProblem("Only approved vacations can be cancelled");

        vacation.Status = VacationStatus.Cancelled;
        vacation.UpdatedAt = DateTime.UtcNow;

        var updated = await _vacationRepository.UpdateAsync(vacation);
        _logger.LogInformation("Vacation {VacationId} cancelled by {UserId}", id, user.Id);

        return Ok(_mapper.Map<VacationDto>(updated));
    }
}
