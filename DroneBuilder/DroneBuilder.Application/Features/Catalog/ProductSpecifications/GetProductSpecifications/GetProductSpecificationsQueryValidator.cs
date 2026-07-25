using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.GetProductSpecifications;

public sealed class GetProductSpecificationsQueryValidator
    : AbstractValidator<GetProductSpecificationsQuery>
{
    public GetProductSpecificationsQueryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
    }
}
