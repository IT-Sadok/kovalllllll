using FluentValidation;

namespace DroneBuilder.Application.Features.Inventory.GetWarehouseItems;

public class GetWarehouseItemsQueryValidator : AbstractValidator<GetWarehouseItemsQuery>
{
    public GetWarehouseItemsQueryValidator()
    {
        RuleFor(x => x.Pagination).NotNull().WithMessage("Pagination is required.");
        RuleFor(x => x.Pagination.Page).GreaterThan(0).When(x => x.Pagination != null).WithMessage("Page must be greater than 0.");
        RuleFor(x => x.Pagination.PageSize).InclusiveBetween(1, 100).When(x => x.Pagination != null)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}

