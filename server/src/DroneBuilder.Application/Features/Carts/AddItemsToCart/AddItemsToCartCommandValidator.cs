using DroneBuilder.Application.Features.Builds;
using FluentValidation;

namespace DroneBuilder.Application.Features.Carts.AddItemsToCart;

public class AddItemsToCartCommandValidator : AbstractValidator<AddItemsToCartCommand>
{
    public AddItemsToCartCommandValidator()
    {
        this.RuleForBuildItems(x => x.Items);
    }
}
