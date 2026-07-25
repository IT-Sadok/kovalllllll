using FluentValidation;

namespace DroneBuilder.Application.Features.Inventory.RemoveQuantityFromWarehouseItem;

public class RemoveQuantityFromWarehouseItemCommandValidator : AbstractValidator<RemoveQuantityFromWarehouseItemCommand>
{
    public RemoveQuantityFromWarehouseItemCommandValidator()
    {
        RuleFor(x => x.WarehouseItemId)
            .NotEmpty().WithMessage("Warehouse item ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Quantity data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.QuantityToRemove)
                .GreaterThan(0).WithMessage("Quantity to remove must be greater than 0.");
        });
    }
}
