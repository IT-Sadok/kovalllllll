using DroneBuilder.Application.Mediator.Commands.UserCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("Confirmation token is required.");
    }
}
