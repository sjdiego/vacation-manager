using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using AutoMapper;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;
using VacationManager.Core.Validation;
using VacationManager.Core.Specifications;
using VacationManager.Core.Authorization;
using VacationManager.Core.Authorization.Handlers;
using VacationManager.Api.Helpers;
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
    private readonly IVacationValidationService _validationService;
    private readonly IVacationAuthorizationHelper _authHelper;

    public VacationsController(
        IVacationRepository vacationRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<VacationsController> logger,
        IVacationValidationService validationService,
        IVacationAuthorizationHelper authHelper)
    {
        _vacationRepository = vacationRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
        _validationService = validationService;
        _authHelper = authHelper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VacationDto>>> GetMyVacations()
    {
        var (user, authResult) = await _authHelper.AuthorizeUserAsync(User);
        if (!authResult.IsAuthorized || user == null)
            return this.BadRequestProblem(authResult.FailureReason!);

        var vacations = await _vacationRepository.GetByUserIdAsync(user.Id);
        return Ok(_mapper.Map<List<VacationDto>>(vacations));
    }

    [HttpGet("team/pending")]
    public async Task<ActionResult<IEnumerable<VacationDto>>> GetTeamPendingVacations()
    {
        var (user, authResult) = await _authHelper.AuthorizeManagerOperationAsync(User);
        if (!authResult.IsAuthorized || user == null)
            return this.ForbiddenProblem(authResult.FailureReason!);

        var vacations = await _vacationRepository.GetByTeamAsync(user.TeamId!.Value);
        var pendingSpec = new PendingVacationsSpecification();
        var pendingVacations = vacations.Where(pendingSpec).ToList();
        
        return Ok(_mapper.Map<List<VacationDto>>(pendingVacations));
    }

    [HttpGet("team")]
    public async Task<ActionResult<IEnumerable<VacationDto>>> GetTeamVacations([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var (user, authResult) = await _authHelper.AuthorizeTeamOperationAsync(User);
        if (!authResult.IsAuthorized || user == null)
            return this.BadRequestProblem(authResult.FailureReason!);

        var vacations = await _vacationRepository.GetByTeamAsync(user.TeamId!.Value);
        
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
        var (user, authResult) = await _authHelper.AuthorizeTeamOperationAsync(User);
        if (!authResult.IsAuthorized || user == null)
            return this.BadRequestProblem(authResult.FailureReason!);

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

        var (user, authResult) = await _authHelper.AuthorizeVacationOwnershipAsync(User, vacation);
        if (!authResult.IsAuthorized || user == null)
            return this.ForbiddenProblem(authResult.FailureReason!);

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

        var (user, authResult) = await _authHelper.AuthorizeVacationOwnershipAsync(User, vacation);
        if (!authResult.IsAuthorized || user == null)
            return this.ForbiddenProblem(authResult.FailureReason!);

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

        var vacationOwner = await _userRepository.GetByIdAsync(vacation.UserId);
        if (vacationOwner == null)
            return NotFound("Vacation owner not found");

        var (manager, authResult) = await _authHelper.AuthorizeApprovalAsync(User, vacationOwner.TeamId!.Value);
        if (!authResult.IsAuthorized || manager == null)
            return this.ForbiddenProblem(authResult.FailureReason!);

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

        var vacationOwner = await _userRepository.GetByIdAsync(vacation.UserId);
        if (vacationOwner == null)
            return NotFound("Vacation owner not found");

        var (user, authResult) = await _authHelper.AuthorizeVacationOwnershipAsync(
            User, vacation, vacationOwner.TeamId);
        if (!authResult.IsAuthorized || user == null)
            return this.ForbiddenProblem(authResult.FailureReason!);

        if (vacation.Status != VacationStatus.Approved)
            return this.BadRequestProblem("Only approved vacations can be cancelled");

        vacation.Status = VacationStatus.Cancelled;
        vacation.UpdatedAt = DateTime.UtcNow;

        var updated = await _vacationRepository.UpdateAsync(vacation);
        _logger.LogInformation("Vacation {VacationId} cancelled by {UserId}", id, user.Id);

        return Ok(_mapper.Map<VacationDto>(updated));
    }
}
