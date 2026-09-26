using FluentValidation;

namespace DroneBuilder.Application.Features.Products.GetProducts;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Pagination).NotNull().WithMessage("Pagination is required.");
        RuleFor(x => x.Pagination.Page).GreaterThan(0).When(x => x.Pagination != null).WithMessage("Page must be greater than 0.");
        RuleFor(x => x.Pagination.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.")
            .When(x => x.Pagination != null);

        RuleFor(x => x.Filter.Cells).InclusiveBetween(1, 14).When(x => x.Filter.Cells.HasValue)
            .WithMessage("Cells must be between 1 and 14.");
        RuleFor(x => x.Filter.PropSizeInch).GreaterThan(0).When(x => x.Filter.PropSizeInch.HasValue)
            .WithMessage("Prop size must be greater than 0.");
        RuleFor(x => x.Filter.KvMax).GreaterThanOrEqualTo(x => x.Filter.KvMin!.Value)
            .When(x => x.Filter.KvMin.HasValue && x.Filter.KvMax.HasValue)
            .WithMessage("KV max must not be lower than KV min.");
        RuleFor(x => x.Filter.CapacityMax).GreaterThanOrEqualTo(x => x.Filter.CapacityMin!.Value)
            .When(x => x.Filter.CapacityMin.HasValue && x.Filter.CapacityMax.HasValue)
            .WithMessage("Capacity max must not be lower than capacity min.");
        RuleFor(x => x.Filter.Category).NotNull().When(x => x.Filter.CompatibleWith is { Length: > 0 })
            .WithMessage("Choose a category to find compatible parts.");
        RuleFor(x => x.Filter.CompatibleWith).Must(ids => ids!.Length <= 30).When(x => x.Filter.CompatibleWith != null)
            .WithMessage("Compatibility can be checked against at most 30 parts.");
    }
}

