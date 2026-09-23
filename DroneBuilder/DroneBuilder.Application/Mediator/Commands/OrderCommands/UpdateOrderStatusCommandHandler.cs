using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IWarehouseRepository warehouseRepository)
    : ICommandHandler<UpdateOrderStatusCommand>
{
    private static readonly Dictionary<Status, Status[]> AllowedTransitions = new()
    {
        [Status.New] = [Status.Paid, Status.Cancelled],
        [Status.Paid] = [Status.Sent, Status.Cancelled],
        [Status.Sent] = [Status.Completed],
        [Status.Completed] = [],
        [Status.Cancelled] = []
    };

    public async Task<Result> ExecuteCommandAsync(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetOrderByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Fail(new NotFoundError($"Order with ID {command.OrderId} not found."));
        }

        if (!Enum.IsDefined(typeof(Status), command.NewStatus))
        {
            return Result.Fail(new BadRequestError($"Invalid status value: {command.NewStatus}"));
        }

        if (!AllowedTransitions.TryGetValue(order.Status, out Status[]? allowedStatuses)
            || !allowedStatuses.Contains(command.NewStatus))
        {
            return Result.Fail(new BadRequestError(
                $"Cannot change order status from {order.Status} to {command.NewStatus}."));
        }

        if (command.NewStatus == Status.Cancelled)
        {
            Result restockResult = await RestockAsync(order, cancellationToken);
            if (restockResult.IsFailed)
            {
                return restockResult;
            }
        }

        order.Status = command.NewStatus;

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task<Result> RestockAsync(Order order, CancellationToken cancellationToken)
    {
        foreach (OrderItem item in order.OrderItems)
        {
            WarehouseItem? warehouseItem =
                await warehouseRepository.GetWarehouseItemByProductIdAsync(item.ProductId, cancellationToken);

            if (warehouseItem is null)
            {
                return Result.Fail(new NotFoundError(
                    $"Warehouse item for product {item.ProductId} not found while cancelling the order."));
            }

            warehouseItem.Quantity += item.Quantity;
        }

        return Result.Ok();
    }
}

public record UpdateOrderStatusCommand(Guid OrderId, Status NewStatus);
