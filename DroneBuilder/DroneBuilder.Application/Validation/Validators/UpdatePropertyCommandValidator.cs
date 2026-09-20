using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Property data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Name)
                .MaximumLength(100).WithMessage("Property name must not exceed 100 characters.")
                .When(x => x.Model.Name != null);
        });
    }
}
