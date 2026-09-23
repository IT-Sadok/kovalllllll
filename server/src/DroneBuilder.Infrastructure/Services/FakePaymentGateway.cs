using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Application.Features.Orders;
using DroneBuilder.Domain.Entities;
using FluentResults;
using Microsoft.Extensions.Logging;
namespace DroneBuilder.Infrastructure.Services;

public class FakePaymentGateway(ILogger<FakePaymentGateway> logger) : IPaymentGateway
{
    public Task<PaymentSession> CreateCheckoutSessionAsync(Order order, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Fake payment gateway marks order {OrderId} as paid without charging anyone", order.Id);

        var session = new PaymentSession($"fake_{Guid.NewGuid():N}", null, PaymentSessionStatus.Paid,
            order.TotalPrice.ToMinorUnits());

        return Task.FromResult(session);
    }

    public Task<PaymentSession?> GetCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        => Task.FromResult<PaymentSession?>(null);

    public Task ExpireCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Result<PaymentNotification?> ParseNotification(string payload, string signature)
        => Result.Fail<PaymentNotification?>(new BadRequestError("The fake payment gateway does not accept webhooks."));
}
