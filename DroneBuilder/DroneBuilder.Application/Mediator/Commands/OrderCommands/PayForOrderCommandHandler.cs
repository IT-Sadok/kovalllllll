using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public class PayForOrderCommandHandler(IOrderRepository orderRepository) : ICommandHandler<PayForOrderCommand>
{
    public async Task ExecuteCommandAsync(PayForOrderCommand payForOrderCommand, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetOrderByIdAsync(payForOrderCommand.OrderId, cancellationToken);
        if (order is null)
            throw new NotFoundException($"Order with id {payForOrderCommand.OrderId} not found.");

        if (order.Status != Status.New)
            throw new BadRequestException("Order is not in new status.");
        if (order.Status == Status.Paid)
            throw new BadRequestException("Order is already paid.");

        order.Status = Status.Paid;

        await orderRepository.SaveChangesAsync(cancellationToken);
    }
}

public record PayForOrderCommand(Guid OrderId);