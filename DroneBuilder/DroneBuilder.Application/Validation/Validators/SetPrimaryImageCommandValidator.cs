using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class SetPrimaryImageCommandValidator : AbstractValidator<SetPrimaryImageCommand>
{
    public SetPrimaryImageCommandValidator()
    {
        RuleFor(x => x.ImageId).NotEmpty().WithMessage("ImageId is required.");
    }
}


