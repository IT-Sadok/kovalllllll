using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Orders.ConfirmOrderPayment;

public class ConfirmOrderPaymentCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<ConfirmOrderPaymentCommand>
{
    public async Task<Result> ExecuteCommandAsync(ConfirmOrderPaymentCommand command,
        CancellationToken cancellationToken)
    {
        PaymentNotification notification = command.Notification;

        if (!notification.IsPaid)
        {
            return Result.Ok();
        }

        Order? order = await orderRepository.GetOrderByIdAsync(notification.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Fail(new NotFoundError($"Order with id {notification.OrderId} not found."));
        }

        Result paidResult = order.MarkPaid(notification.SessionId, notification.AmountTotal);
        if (paidResult.IsFailed)
        {
            return paidResult;
        }

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record ConfirmOrderPaymentCommand(PaymentNotification Notification);
