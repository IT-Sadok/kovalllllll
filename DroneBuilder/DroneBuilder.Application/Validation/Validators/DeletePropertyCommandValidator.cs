using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class DeletePropertyCommandValidator : AbstractValidator<DeletePropertyCommand>
{
    public DeletePropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
    }
}


