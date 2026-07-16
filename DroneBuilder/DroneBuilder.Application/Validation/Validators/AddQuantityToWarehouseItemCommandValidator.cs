using DroneBuilder.Application.Mediator.Commands.WarehouseCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class AddQuantityToWarehouseItemCommandValidator : AbstractValidator<AddQuantityToWarehouseItemCommand>
{
    public AddQuantityToWarehouseItemCommandValidator()
    {
        RuleFor(x => x.WarehouseItemId)
            .NotEmpty().WithMessage("Warehouse item ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Quantity data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.QuantityToAdd)
                .GreaterThan(0).WithMessage("Quantity to add must be greater than 0.");
        });
    }
}
