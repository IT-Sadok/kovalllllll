using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public class StartOrderPaymentCommandHandler(
    IOrderRepository orderRepository,
    IPaymentGateway paymentGateway,
    IUserContext userContext)
    : ICommandHandler<StartOrderPaymentCommand, PaymentSessionModel>
{
    public async Task<Result<PaymentSessionModel>> ExecuteCommandAsync(StartOrderPaymentCommand command,
        CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetOrderByIdAsync(command.OrderId, cancellationToken);

        if (order is null || order.UserId != userContext.UserId)
        {
            return Result.Fail<PaymentSessionModel>(new NotFoundError($"Order with id {command.OrderId} not found."));
        }

        if (order.Status == Status.Paid)
        {
            return Result.Fail<PaymentSessionModel>(new BadRequestError("Order is already paid."));
        }

        if (order.Status != Status.New)
        {
            return Result.Fail<PaymentSessionModel>(new BadRequestError("Only new orders can be paid."));
        }

        PaymentSession? session = order.PaymentSessionId is null
            ? null
            : await paymentGateway.GetCheckoutSessionAsync(order.PaymentSessionId, cancellationToken);

        if (session?.Status == PaymentSessionStatus.Processing)
        {
            return Result.Fail<PaymentSessionModel>(new BadRequestError("A payment for this order is still being processed."));
        }

        if (session is null || session.Status == PaymentSessionStatus.Expired)
        {
            session = await paymentGateway.CreateCheckoutSessionAsync(order, cancellationToken);
            order.PaymentSessionId = session.Id;
        }

        if (session.Status == PaymentSessionStatus.Paid)
        {
            Result paidResult = order.MarkPaid(session.Id, session.AmountTotal);
            if (paidResult.IsFailed)
            {
                return paidResult.ToResult<PaymentSessionModel>();
            }
        }

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(new PaymentSessionModel
        {
            Url = session.Status == PaymentSessionStatus.Open ? session.Url : null,
            IsPaid = order.Status == Status.Paid
        });
    }
}

public record StartOrderPaymentCommand(Guid OrderId);
