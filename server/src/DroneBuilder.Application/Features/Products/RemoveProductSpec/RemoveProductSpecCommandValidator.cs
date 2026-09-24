using FluentValidation;

namespace DroneBuilder.Application.Features.Products.RemoveProductSpec;

public class RemoveProductSpecCommandValidator : AbstractValidator<RemoveProductSpecCommand>
{
    public RemoveProductSpecCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
    }
}
