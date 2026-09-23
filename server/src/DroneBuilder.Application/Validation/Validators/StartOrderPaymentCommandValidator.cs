using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class StartOrderPaymentCommandValidator : AbstractValidator<StartOrderPaymentCommand>
{
    public StartOrderPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
    }
}

