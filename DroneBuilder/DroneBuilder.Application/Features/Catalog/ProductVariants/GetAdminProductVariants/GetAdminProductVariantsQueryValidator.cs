using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductVariants.GetAdminProductVariants;

public sealed class GetAdminProductVariantsQueryValidator : AbstractValidator<GetAdminProductVariantsQuery>
{
    public GetAdminProductVariantsQueryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
    }
}
