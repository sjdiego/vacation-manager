using FluentValidation;
using VacationManager.Core.DTOs;

namespace VacationManager.Core.Validators;

public class CreateVacationDtoValidator : AbstractValidator<CreateVacationDto>
{
    public CreateVacationDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after or equal to start date");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Vacation type is invalid");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }
}

public class UpdateVacationDtoValidator : AbstractValidator<UpdateVacationDto>
{
    public UpdateVacationDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after or equal to start date");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Vacation type is invalid");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }
}

public class ApproveVacationDtoValidator : AbstractValidator<ApproveVacationDto>
{
    public ApproveVacationDtoValidator()
    {
        When(x => !x.Approved, () =>
        {
            RuleFor(x => x.RejectReason)
                .NotEmpty().WithMessage("Rejection reason is required when rejecting a vacation")
                .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");
        });
    }
}
