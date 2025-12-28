using VacationManager.Core.Entities;

namespace VacationManager.Core.Validation;

/// <summary>
/// Defines a contract for vacation validation rules
/// </summary>
public interface IVacationValidationRule
{
    /// <summary>
    /// Validates a vacation request against a specific business rule
    /// </summary>
    /// <param name="vacation">The vacation to validate</param>
    /// <param name="user">The user requesting the vacation</param>
    /// <returns>A validation result indicating success or failure with an error message</returns>
    Task<ValidationResult> ValidateAsync(Vacation vacation, User user);

    /// <summary>
    /// The order in which this rule should be executed (lower numbers execute first)
    /// </summary>
    int Order { get; }
}
