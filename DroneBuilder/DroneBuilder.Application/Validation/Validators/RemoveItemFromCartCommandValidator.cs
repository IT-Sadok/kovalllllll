using DroneBuilder.Application.Mediator.Commands.CartCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class RemoveItemFromCartCommandValidator : AbstractValidator<RemoveItemFromCartCommand>
{
    public RemoveItemFromCartCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
    }
}

