using FluentValidation;
using VacationManager.Core.DTOs;

namespace VacationManager.Core.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(256).WithMessage("Display name cannot exceed 256 characters")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters");

        RuleFor(x => x.Department)
            .MaximumLength(256).WithMessage("Department cannot exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.TeamId)
            .NotEqual(Guid.Empty).WithMessage("Team ID must be a valid GUID")
            .When(x => x.TeamId.HasValue);
    }
}

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(256).WithMessage("Display name cannot exceed 256 characters")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters")
            .When(x => !string.IsNullOrEmpty(x.DisplayName));

        RuleFor(x => x.Department)
            .MaximumLength(256).WithMessage("Department cannot exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.TeamId)
            .NotEqual(Guid.Empty).WithMessage("Team ID must be a valid GUID")
            .When(x => x.TeamId.HasValue);
    }
}
