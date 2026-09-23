using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Features.Orders.ConfirmOrderPayment;
using DroneBuilder.Application.Mediator.Interfaces;
using FluentResults;
namespace DroneBuilder.API.Features.Orders;

public static class PaymentEndpointExtensions
{
    private const string StripeSignatureHeader = "Stripe-Signature";

    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Payments.StripeWebhook,
                async (HttpContext httpContext, IPaymentGateway paymentGateway, IMediator mediator,
                    ILogger<Program> logger, CancellationToken cancellationToken) =>
                {
                    string payload;
                    using (var reader = new StreamReader(httpContext.Request.Body))
                    {
                        payload = await reader.ReadToEndAsync(cancellationToken);
                    }

                    string signature = httpContext.Request.Headers[StripeSignatureHeader].ToString();

                    Result<PaymentNotification?> parsed = paymentGateway.ParseNotification(payload, signature);
                    if (parsed.IsFailed)
                    {
                        return Results.BadRequest();
                    }

                    if (parsed.Value is null)
                    {
                        return Results.Ok();
                    }

                    Result result = await mediator.ExecuteCommandAsync(
                        new ConfirmOrderPaymentCommand(parsed.Value),
                        cancellationToken);

                    if (result.IsFailed)
                    {
                        logger.LogError("Payment {SessionId} for order {OrderId} was not applied: {Reason}",
                            parsed.Value.SessionId, parsed.Value.OrderId, result.Errors[0].Message);
                    }

                    return Results.Ok();
                })
            .WithTags("Payments")
            .AllowAnonymous()
            .ExcludeFromDescription();

        return app;
    }
}
