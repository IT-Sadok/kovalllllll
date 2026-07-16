using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class RemoveValueFromPropertyCommandValidator : AbstractValidator<RemoveValueFromPropertyCommand>
{
    public RemoveValueFromPropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}


