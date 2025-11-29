using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.OrderQueries;
using DroneBuilder.Application.Models.OrderModels;

namespace DroneBuilder.API.Endpoints;

public static class OrderEndpointExtensions
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Orders.CreateOrder,
                async (IMediator mediator, IUserContext userContext, ShippingDetailsModel shippingDetails,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteCommandAsync<CreateOrderCommand, OrderModel>(
                        new CreateOrderCommand(userContext.UserId, shippingDetails),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Orders")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Orders.GetAllOrders,
                async (IMediator mediator, IUserContext userContext, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetOrdersQuery, ICollection<OrderModel>>(
                        new GetOrdersQuery(userContext.UserId),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Orders")
            .RequireAuthorization();

        return app;
    }
}