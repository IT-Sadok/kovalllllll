using DroneBuilder.Application.Mediator.Commands.UserCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class SignUpUserCommandValidator : AbstractValidator<SignUpUserCommand>
{
    public SignUpUserCommandValidator()
    {
        RuleFor(x => x.Model)
            .NotNull().WithMessage("Sign-up data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Model.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        });
    }
}
