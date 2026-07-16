using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class DeleteValueCommandValidator : AbstractValidator<DeleteValueCommand>
{
    public DeleteValueCommandValidator()
    {
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}

