using FluentValidation;

namespace DroneBuilder.Application.Features.Orders.StartOrderPayment;

public class StartOrderPaymentCommandValidator : AbstractValidator<StartOrderPaymentCommand>
{
    public StartOrderPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
    }
}

