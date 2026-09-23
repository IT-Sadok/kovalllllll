using DroneBuilder.Application.Mediator.Commands.UserCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class ResendEmailConfirmationCommandValidator : AbstractValidator<ResendEmailConfirmationCommand>
{
    public ResendEmailConfirmationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
