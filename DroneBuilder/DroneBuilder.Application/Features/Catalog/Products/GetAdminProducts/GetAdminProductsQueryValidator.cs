using DroneBuilder.Domain.Entities;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Products.GetAdminProducts;

public sealed class GetAdminProductsQueryValidator : AbstractValidator<GetAdminProductsQuery>
{
    public GetAdminProductsQueryValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
        RuleFor(query => query.Pagination.Page).GreaterThan(0)
            .When(query => query.Pagination is not null);
        RuleFor(query => query.Pagination.PageSize).InclusiveBetween(1, 100)
            .When(query => query.Pagination is not null);
        RuleFor(query => query.Filter).NotNull();
        RuleFor(query => query.Filter.PublicationStatus)
            .Must(status => Enum.TryParse<ProductPublicationStatus>(status, true, out _))
            .WithMessage("Unsupported publication status.")
            .When(query => query.Filter?.PublicationStatus is not null);
        RuleFor(query => query.Filter)
            .Must(filter => !filter.MinPrice.HasValue || !filter.MaxPrice.HasValue ||
                            filter.MinPrice <= filter.MaxPrice)
            .WithMessage("MinPrice cannot be greater than MaxPrice.")
            .When(query => query.Filter is not null);
    }
}
