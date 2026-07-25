using FluentValidation;

namespace DroneBuilder.Application.Features.Orders.PayForOrder;

public class PayForOrderCommandValidator : AbstractValidator<PayForOrderCommand>
{
    public PayForOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
    }
}

