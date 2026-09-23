using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Common.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentSession> CreateCheckoutSessionAsync(Order order, CancellationToken cancellationToken = default);

    Task<PaymentSession?> GetCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    Task ExpireCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    Result<PaymentNotification?> ParseNotification(string payload, string signature);
}

public enum PaymentSessionStatus
{
    Open,
    Paid,
    Processing,
    Expired
}

public record PaymentSession(string Id, string? Url, PaymentSessionStatus Status, long AmountTotal);

public record PaymentNotification(string SessionId, Guid OrderId, bool IsPaid, long AmountTotal);
