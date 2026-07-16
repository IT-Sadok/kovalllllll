using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class CreateValueCommandValidator : AbstractValidator<CreateValueCommand>
{
    public CreateValueCommandValidator()
    {
        RuleFor(x => x.Model)
            .NotNull().WithMessage("Value data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Text)
                .NotEmpty().WithMessage("Value text is required.")
                .MaximumLength(500).WithMessage("Value text must not exceed 500 characters.");

            RuleFor(x => x.Model.PropertyId)
                .NotEmpty().WithMessage("Property ID is required.");
        });
    }
}
