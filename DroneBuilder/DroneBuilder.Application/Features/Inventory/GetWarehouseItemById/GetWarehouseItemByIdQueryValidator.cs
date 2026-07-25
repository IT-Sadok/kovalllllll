using FluentValidation;

namespace DroneBuilder.Application.Features.Inventory.GetWarehouseItemById;

public class GetWarehouseItemByIdQueryValidator : AbstractValidator<GetWarehouseItemByIdQuery>
{
    public GetWarehouseItemByIdQueryValidator()
    {
        RuleFor(x => x.WarehouseItemId).NotEmpty().WithMessage("WarehouseItemId is required.");
    }
}

