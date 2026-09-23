using DroneBuilder.Application.Mediator.Queries.WarehouseQueries;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class GetWarehouseItemByIdQueryValidator : AbstractValidator<GetWarehouseItemByIdQuery>
{
    public GetWarehouseItemByIdQueryValidator()
    {
        RuleFor(x => x.WarehouseItemId).NotEmpty().WithMessage("WarehouseItemId is required.");
    }
}

