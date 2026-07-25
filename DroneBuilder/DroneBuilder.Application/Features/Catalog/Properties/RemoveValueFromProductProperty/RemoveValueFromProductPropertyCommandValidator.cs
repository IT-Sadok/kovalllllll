using DroneBuilder.Application.Features.Catalog.Products.RemoveValueFromProductProperty;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Properties.RemoveValueFromProductProperty;

public class RemoveValueFromProductPropertyCommandValidator : AbstractValidator<RemoveValueFromProductPropertyCommand>
{
    public RemoveValueFromProductPropertyCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}

