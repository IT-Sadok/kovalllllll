using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.GetAdminProductSpecifications;

public sealed class GetAdminProductSpecificationsQueryValidator
    : AbstractValidator<GetAdminProductSpecificationsQuery>
{
    public GetAdminProductSpecificationsQueryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
    }
}
