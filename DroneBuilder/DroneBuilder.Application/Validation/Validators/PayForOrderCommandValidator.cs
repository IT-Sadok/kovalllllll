using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class PayForOrderCommandValidator : AbstractValidator<PayForOrderCommand>
{
    public PayForOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
    }
}


