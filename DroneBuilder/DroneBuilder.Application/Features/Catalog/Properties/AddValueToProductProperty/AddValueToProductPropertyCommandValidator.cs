using DroneBuilder.Application.Features.Catalog.Products.AddValueToProductProperty;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Properties.AddValueToProductProperty;

public class AddValueToProductPropertyCommandValidator : AbstractValidator<AddValueToProductPropertyCommand>
{
    public AddValueToProductPropertyCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
        RuleFor(x => x.ValueId).NotEmpty().WithMessage("ValueId is required.");
    }
}

