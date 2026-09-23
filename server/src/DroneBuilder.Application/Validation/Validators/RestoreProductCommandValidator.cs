using DroneBuilder.Application.Mediator.Commands.ProductCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class RestoreProductCommandValidator : AbstractValidator<RestoreProductCommand>
{
    public RestoreProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
    }
}
