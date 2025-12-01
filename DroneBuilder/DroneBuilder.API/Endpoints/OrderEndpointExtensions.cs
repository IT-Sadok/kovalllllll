using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.OrderQueries;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.OrderModels;

namespace DroneBuilder.API.Endpoints;

public static class OrderEndpointExtensions
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Orders.CreateOrder,
                async (IMediator mediator, ShippingDetailsModel shippingDetails,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteCommandAsync<CreateOrderCommand, OrderModel>(
                        new CreateOrderCommand(shippingDetails),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Orders")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Orders.GetAllOrders,
                async (int page, int pageSize, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetOrdersQuery, PagedResult<OrderModel>>(
                        new GetOrdersQuery(page, pageSize),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Orders")
            .RequireAuthorization();

        return app;
    }
}