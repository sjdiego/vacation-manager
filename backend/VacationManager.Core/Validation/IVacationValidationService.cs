using VacationManager.Core.Entities;

namespace VacationManager.Core.Validation;

/// <summary>
/// Service interface for validating vacation requests against business rules
/// </summary>
public interface IVacationValidationService
{
    /// <summary>
    /// Validates a vacation against all registered rules in order
    /// </summary>
    Task<ValidationResult> ValidateAsync(Vacation vacation, User user);

    /// <summary>
    /// Validates a vacation against all rules and returns all validation failures
    /// </summary>
    Task<IEnumerable<ValidationResult>> ValidateAllAsync(Vacation vacation, User user);
}
