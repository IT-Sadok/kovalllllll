using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.GetVariantSpecifications;

public sealed class GetVariantSpecificationsQueryValidator : AbstractValidator<GetVariantSpecificationsQuery>
{
    public GetVariantSpecificationsQueryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
        RuleFor(query => query.VariantId).NotEmpty();
    }
}
