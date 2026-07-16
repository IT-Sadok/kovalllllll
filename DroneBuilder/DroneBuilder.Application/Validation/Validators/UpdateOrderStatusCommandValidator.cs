using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Invalid status.");
    }
}


