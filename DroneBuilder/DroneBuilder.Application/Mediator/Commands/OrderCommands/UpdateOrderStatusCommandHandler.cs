using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<UpdateOrderStatusCommand>
{
    public async Task<Result> ExecuteCommandAsync(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetOrderByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Fail(new NotFoundError($"Order with ID {command.OrderId} not found."));
        }

        // Validate if the status is a valid enum value
        if (!Enum.IsDefined(typeof(Status), command.NewStatus))
        {
            return Result.Fail(new BadRequestError($"Invalid status value: {command.NewStatus}"));
        }

        order.Status = command.NewStatus;

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record UpdateOrderStatusCommand(Guid OrderId, Status NewStatus);
