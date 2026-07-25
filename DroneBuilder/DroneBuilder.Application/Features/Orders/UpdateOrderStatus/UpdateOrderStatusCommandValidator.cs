using FluentValidation;

namespace DroneBuilder.Application.Features.Orders.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Invalid status.");
    }
}

