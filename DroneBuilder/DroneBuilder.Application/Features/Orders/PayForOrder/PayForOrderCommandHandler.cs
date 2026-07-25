using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Orders.PayForOrder;

public class PayForOrderCommandHandler(IOrderRepository orderRepository, IUserContext userContext) : ICommandHandler<PayForOrderCommand>
{
    public async Task<Result> ExecuteCommandAsync(PayForOrderCommand payForOrderCommand, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetOrderByIdAsync(payForOrderCommand.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Fail(new NotFoundError($"Order with id {payForOrderCommand.OrderId} not found."));
        }

        if (order.UserId != userContext.UserId)
        {
            return Result.Fail(new ForbiddenError("The order does not belong to the current user."));
        }

        if (order.Status == Status.Paid)
        {
            return Result.Fail(new BadRequestError("Order is already paid."));
        }

        if (order.Status != Status.New)
        {
            return Result.Fail(new BadRequestError("Order is not in new status."));
        }

        try
        {
            order.MarkAsPaid();
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail(new BadRequestError(exception.Message));
        }

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record PayForOrderCommand(Guid OrderId);
