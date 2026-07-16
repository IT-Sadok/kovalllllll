using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Name)
                .MaximumLength(200).WithMessage("Property name must not exceed 200 characters.")
                .When(x => x.Model.Name != null);
        });
    }
}
