using FluentValidation;

namespace DroneBuilder.Application.Features.Products.RemovePropertyFromProduct;

public class RemovePropertyFromProductCommandValidator : AbstractValidator<RemovePropertyFromProductCommand>
{
    public RemovePropertyFromProductCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
        RuleFor(x => x.PropertyId).NotEmpty().WithMessage("PropertyId is required.");
    }
}

