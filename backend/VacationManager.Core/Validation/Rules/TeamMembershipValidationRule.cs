using VacationManager.Core.Entities;

namespace VacationManager.Core.Validation.Rules;

/// <summary>
/// Validates that the user is a member of a team before requesting vacation
/// </summary>
public class TeamMembershipValidationRule : IVacationValidationRule
{
    public int Order => 1;

    public Task<ValidationResult> ValidateAsync(Vacation vacation, User user)
    {
        if (user.TeamId == null)
        {
            return Task.FromResult(
                ValidationResult.Failure(
                    "User must be part of a team to request vacation",
                    "TEAM_MEMBERSHIP_REQUIRED"));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}
