using VacationManager.Core.Entities;

namespace VacationManager.Core.Validation;

/// <summary>
/// Service that orchestrates the execution of validation rules
/// </summary>
public class VacationValidationService : IVacationValidationService
{
    private readonly IEnumerable<IVacationValidationRule> _rules;

    public VacationValidationService(IEnumerable<IVacationValidationRule> rules)
    {
        _rules = rules;
    }

    /// <summary>
    /// Validates a vacation against all registered rules in order
    /// </summary>
    /// <param name="vacation">The vacation to validate</param>
    /// <param name="user">The user requesting the vacation</param>
    /// <returns>The first validation failure encountered, or success if all rules pass</returns>
    public async Task<ValidationResult> ValidateAsync(Vacation vacation, User user)
    {
        var orderedRules = _rules.OrderBy(r => r.Order);

        foreach (var rule in orderedRules)
        {
            var result = await rule.ValidateAsync(vacation, user);
            if (!result.IsValid)
            {
                return result;
            }
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates a vacation against all rules and returns all validation failures
    /// </summary>
    /// <param name="vacation">The vacation to validate</param>
    /// <param name="user">The user requesting the vacation</param>
    /// <returns>A list of all validation failures</returns>
    public async Task<IEnumerable<ValidationResult>> ValidateAllAsync(Vacation vacation, User user)
    {
        var orderedRules = _rules.OrderBy(r => r.Order);
        var failures = new List<ValidationResult>();

        foreach (var rule in orderedRules)
        {
            var result = await rule.ValidateAsync(vacation, user);
            if (!result.IsValid)
            {
                failures.Add(result);
            }
        }

        return failures;
    }
}
