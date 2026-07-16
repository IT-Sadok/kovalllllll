using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class DeleteImageCommandValidator : AbstractValidator<DeleteImageCommand>
{
    public DeleteImageCommandValidator()
    {
        RuleFor(x => x.ImageId).NotEmpty().WithMessage("ImageId is required.");
    }
}

