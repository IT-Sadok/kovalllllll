using FluentValidation;

namespace DroneBuilder.Application.Features.Warehouses.GetWarehouseItemById;

public class GetWarehouseItemByIdQueryValidator : AbstractValidator<GetWarehouseItemByIdQuery>
{
    public GetWarehouseItemByIdQueryValidator()
    {
        RuleFor(x => x.WarehouseItemId).NotEmpty().WithMessage("WarehouseItemId is required.");
    }
}

