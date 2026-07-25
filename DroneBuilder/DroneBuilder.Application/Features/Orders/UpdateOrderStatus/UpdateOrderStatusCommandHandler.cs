using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Orders.UpdateOrderStatus;

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

        try
        {
            order.ChangeStatus(command.NewStatus);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return Result.Fail(new BadRequestError(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail(new ConflictError(exception.Message));
        }

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record UpdateOrderStatusCommand(Guid OrderId, Status NewStatus);
