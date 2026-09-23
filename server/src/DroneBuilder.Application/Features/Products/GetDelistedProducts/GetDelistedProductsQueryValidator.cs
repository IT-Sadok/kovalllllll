using FluentValidation;

namespace DroneBuilder.Application.Features.Products.GetDelistedProducts;

public class GetDelistedProductsQueryValidator : AbstractValidator<GetDelistedProductsQuery>
{
    public GetDelistedProductsQueryValidator()
    {
        RuleFor(x => x.Pagination).NotNull().WithMessage("Pagination is required.");
        RuleFor(x => x.Pagination.Page).GreaterThan(0).When(x => x.Pagination != null).WithMessage("Page must be greater than 0.");
        RuleFor(x => x.Pagination.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.")
            .When(x => x.Pagination != null);
    }
}
