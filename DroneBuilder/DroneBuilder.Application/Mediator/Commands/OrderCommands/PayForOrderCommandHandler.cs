using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public class PayForOrderCommandHandler(IOrderRepository orderRepository, IUserContext userContext)
    : ICommandHandler<PayForOrderCommand>
{
    public async Task<Result> ExecuteCommandAsync(PayForOrderCommand payForOrderCommand, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetOrderByIdAsync(payForOrderCommand.OrderId, cancellationToken);

        // Orders that belong to another user are reported as missing so their ids cannot be probed.
        if (order is null || order.UserId != userContext.UserId)
        {
            return Result.Fail(new NotFoundError($"Order with id {payForOrderCommand.OrderId} not found."));
        }

        if (order.Status == Status.Paid)
        {
            return Result.Fail(new BadRequestError("Order is already paid."));
        }

        if (order.Status != Status.New)
        {
            return Result.Fail(new BadRequestError("Order is not in new status."));
        }

        order.Status = Status.Paid;

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record PayForOrderCommand(Guid OrderId);
