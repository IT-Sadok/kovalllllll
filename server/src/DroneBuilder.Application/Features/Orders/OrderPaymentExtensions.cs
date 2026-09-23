using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Orders;

public static class OrderPaymentExtensions
{
    public static long ToMinorUnits(this decimal amount)
        => (long)decimal.Round(amount * 100, MidpointRounding.AwayFromZero);

    public static async Task<Result> ClosePendingPaymentAsync(
        this IPaymentGateway paymentGateway,
        Order order,
        CancellationToken cancellationToken)
    {
        if (order.Status != Status.New || order.PaymentSessionId is null)
        {
            return Result.Ok();
        }

        PaymentSession? session = await paymentGateway.GetCheckoutSessionAsync(order.PaymentSessionId, cancellationToken);

        if (session?.Status is PaymentSessionStatus.Paid or PaymentSessionStatus.Processing)
        {
            return Result.Fail(new BadRequestError("The order has already been paid and can no longer be cancelled here."));
        }

        if (session?.Status == PaymentSessionStatus.Open)
        {
            await paymentGateway.ExpireCheckoutSessionAsync(session.Id, cancellationToken);
        }

        return Result.Ok();
    }

    public static Result MarkPaid(this Order order, string sessionId, long amountPaid)
    {
        if (order.PaymentSessionId != sessionId)
        {
            return Result.Fail(new BadRequestError(
                $"Payment session {sessionId} does not belong to order {order.Id}."));
        }

        if (order.Status == Status.Paid)
        {
            return Result.Ok();
        }

        if (order.Status != Status.New)
        {
            return Result.Fail(new ConflictError(
                $"Payment received for order {order.Id} in status {order.Status}; it needs a manual refund."));
        }

        if (amountPaid != order.TotalPrice.ToMinorUnits())
        {
            return Result.Fail(new ConflictError(
                $"Payment amount {amountPaid} does not match order {order.Id} total {order.TotalPrice.ToMinorUnits()}."));
        }

        order.Status = Status.Paid;

        return Result.Ok();
    }
}
