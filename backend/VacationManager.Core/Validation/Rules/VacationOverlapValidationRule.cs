using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;

namespace VacationManager.Core.Validation.Rules;

/// <summary>
/// Validates that the vacation request doesn't overlap with existing approved vacations
/// </summary>
public class VacationOverlapValidationRule : IVacationValidationRule
{
    private readonly IVacationRepository _vacationRepository;

    public VacationOverlapValidationRule(IVacationRepository vacationRepository)
    {
        _vacationRepository = vacationRepository;
    }

    public int Order => 2;

    public async Task<ValidationResult> ValidateAsync(Vacation vacation, User user)
    {
        var userVacations = await _vacationRepository.GetByUserIdAsync(user.Id);
        
        var hasOverlap = userVacations.Any(v => 
            v.Id != vacation.Id && // Exclude the vacation being updated
            v.Status == VacationStatus.Approved && 
            v.StartDate <= vacation.EndDate && 
            v.EndDate >= vacation.StartDate);

        if (hasOverlap)
        {
            return ValidationResult.Failure(
                "You have overlapping approved vacations in this date range",
                "VACATION_OVERLAP");
        }

        return ValidationResult.Success();
    }
}
