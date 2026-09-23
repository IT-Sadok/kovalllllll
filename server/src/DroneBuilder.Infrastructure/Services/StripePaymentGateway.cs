using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Application.Features.Orders;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure.Options;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
namespace DroneBuilder.Infrastructure.Services;

public class StripePaymentGateway(
    IStripeClient stripeClient,
    IOptions<StripeOptions> options,
    ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    private const string OrderIdMetadataKey = "orderId";

    private readonly StripeOptions _options = options.Value;
    private readonly SessionService _sessionService = new(stripeClient);

    public async Task<PaymentSession> CreateCheckoutSessionAsync(Order order,
        CancellationToken cancellationToken = default)
    {
        var createOptions = new SessionCreateOptions
        {
            Mode = "payment",
            ClientReferenceId = order.Id.ToString(),
            Metadata = new Dictionary<string, string> { [OrderIdMetadataKey] = order.Id.ToString() },
            SuccessUrl = _options.SuccessUrl,
            CancelUrl = _options.CancelUrl,
            AdaptivePricing = new SessionAdaptivePricingOptions { Enabled = false },
            LineItems = order.OrderItems.Select(item => new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = _options.Currency,
                    UnitAmount = item.PriceAtPurchase.ToMinorUnits(),
                    ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.ProductName }
                }
            }).ToList()
        };

        Session session = await _sessionService.CreateAsync(createOptions, cancellationToken: cancellationToken);

        logger.LogInformation("Created Stripe checkout session {SessionId} for order {OrderId}", session.Id, order.Id);

        return ToPaymentSession(session);
    }

    public async Task<PaymentSession?> GetCheckoutSessionAsync(string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Session session = await _sessionService.GetAsync(sessionId, cancellationToken: cancellationToken);
            return ToPaymentSession(session);
        }
        catch (StripeException ex) when (ex.StripeError?.Code == "resource_missing")
        {
            logger.LogWarning("Stripe checkout session {SessionId} no longer exists", sessionId);
            return null;
        }
    }

    public async Task ExpireCheckoutSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await _sessionService.ExpireAsync(sessionId, cancellationToken: cancellationToken);

        logger.LogInformation("Expired Stripe checkout session {SessionId}", sessionId);
    }

    public Result<PaymentNotification?> ParseNotification(string payload, string signature)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Rejected a Stripe webhook with an invalid signature");
            return Result.Fail<PaymentNotification?>(new BadRequestError("Invalid Stripe signature."));
        }

        if (stripeEvent.Type is not (EventTypes.CheckoutSessionCompleted or EventTypes.CheckoutSessionAsyncPaymentSucceeded)
            || stripeEvent.Data.Object is not Session session)
        {
            return Result.Ok<PaymentNotification?>(null);
        }

        if (!session.Metadata.TryGetValue(OrderIdMetadataKey, out string? rawOrderId)
            || !Guid.TryParse(rawOrderId, out Guid orderId))
        {
            logger.LogWarning("Stripe session {SessionId} has no order id; ignoring it", session.Id);
            return Result.Ok<PaymentNotification?>(null);
        }

        return Result.Ok<PaymentNotification?>(new PaymentNotification(
            session.Id,
            orderId,
            session.PaymentStatus == "paid",
            session.AmountTotal ?? 0));
    }

    private static PaymentSession ToPaymentSession(Session session)
    {
        PaymentSessionStatus status = session switch
        {
            { PaymentStatus: "paid" } => PaymentSessionStatus.Paid,
            { Status: "open" } => PaymentSessionStatus.Open,
            { Status: "complete" } => PaymentSessionStatus.Processing,
            _ => PaymentSessionStatus.Expired
        };

        return new PaymentSession(session.Id, session.Url, status, session.AmountTotal ?? 0);
    }
}
