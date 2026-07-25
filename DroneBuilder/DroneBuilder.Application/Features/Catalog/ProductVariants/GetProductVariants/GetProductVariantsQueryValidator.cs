using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.GetProductVariants;

public sealed class GetProductVariantsQueryValidator : AbstractValidator<GetProductVariantsQuery>
{
    public GetProductVariantsQueryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
    }
}
