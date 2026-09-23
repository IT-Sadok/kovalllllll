using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
    }
}

