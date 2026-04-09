using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository) 
    : ICommandHandler<UpdateOrderStatusCommand>
{
    public async Task ExecuteCommandAsync(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetOrderByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException($"Order with ID {command.OrderId} not found.");
        }

        // Validate if the status is a valid enum value
        if (!Enum.IsDefined(typeof(Status), command.NewStatus))
        {
            throw new BadRequestException($"Invalid status value: {command.NewStatus}");
        }

        order.Status = command.NewStatus;

        await orderRepository.SaveChangesAsync(cancellationToken);
    }
}

public record UpdateOrderStatusCommand(Guid OrderId, Status NewStatus);
