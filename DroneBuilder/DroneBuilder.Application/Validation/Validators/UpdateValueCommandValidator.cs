using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class UpdateValueCommandValidator : AbstractValidator<UpdateValueCommand>
{
    public UpdateValueCommandValidator()
    {
        RuleFor(x => x.ValueId)
            .NotEmpty().WithMessage("Value ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Value data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Text)
                .MaximumLength(100).WithMessage("Value text must not exceed 100 characters.")
                .When(x => x.Model.Text != null);
        });
    }
}
