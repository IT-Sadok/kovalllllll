using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class RemovePropertyFromProductCommandValidator : AbstractValidator<RemovePropertyFromProductCommand>
{
    public RemovePropertyFromProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
    }
}

