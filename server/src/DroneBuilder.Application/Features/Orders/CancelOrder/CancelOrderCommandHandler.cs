using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Orders.CancelOrder;

public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IWarehouseRepository warehouseRepository,
    IPaymentGateway paymentGateway,
    IUserContext userContext)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> ExecuteCommandAsync(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetOrderByIdAsync(command.OrderId, cancellationToken);

        if (order is null || order.UserId != userContext.UserId)
        {
            return Result.Fail(new NotFoundError($"Order with id {command.OrderId} not found."));
        }

        if (order.Status != Status.New)
        {
            return Result.Fail(new BadRequestError("Only new orders can be cancelled. Contact support for paid orders."));
        }

        Result paymentResult = await paymentGateway.ClosePendingPaymentAsync(order, cancellationToken);
        if (paymentResult.IsFailed)
        {
            return paymentResult;
        }

        Result restockResult = await warehouseRepository.RestockAsync(order, cancellationToken);
        if (restockResult.IsFailed)
        {
            return restockResult;
        }

        order.Status = Status.Cancelled;

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record CancelOrderCommand(Guid OrderId);
