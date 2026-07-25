using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Products.GetProducts;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Pagination).NotNull().WithMessage("Pagination is required.");
        RuleFor(x => x.Pagination.Page).GreaterThan(0).When(x => x.Pagination != null).WithMessage("Page must be greater than 0.");
        RuleFor(x => x.Pagination.PageSize).InclusiveBetween(1, 100).When(x => x.Pagination != null)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Filter.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Filter.MinPrice.HasValue);
        RuleFor(x => x.Filter.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Filter.MaxPrice.HasValue);
        RuleFor(x => x.Filter)
            .Must(filter => !filter.MinPrice.HasValue || !filter.MaxPrice.HasValue ||
                            filter.MinPrice <= filter.MaxPrice)
            .WithMessage("MinPrice cannot be greater than MaxPrice.");
    }
}

