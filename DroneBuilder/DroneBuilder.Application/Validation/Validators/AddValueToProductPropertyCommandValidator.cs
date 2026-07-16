using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class AddValueToProductPropertyCommandValidator : AbstractValidator<AddValueToProductPropertyCommand>
{
    public AddValueToProductPropertyCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}


