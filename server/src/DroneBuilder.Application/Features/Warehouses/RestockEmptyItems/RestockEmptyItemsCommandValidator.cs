using FluentValidation;

namespace DroneBuilder.Application.Features.Warehouses.RestockEmptyItems;

public class RestockEmptyItemsCommandValidator : AbstractValidator<RestockEmptyItemsCommand>
{
    public RestockEmptyItemsCommandValidator()
    {
        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 10000).WithMessage("Quantity must be between 1 and 10000.");
    }
}
