using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.OrderQueries;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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
                    var pagination = new PaginationParams(page, pageSize);

                    var result = await mediator.ExecuteQueryAsync<GetOrdersQuery, PagedResult<OrderModel>>(
                        new GetOrdersQuery(pagination),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Orders")
            .RequireAuthorization();

        app.MapPatch(ApiRoutes.Orders.PayForOrder,
                async (IMediator mediator, Guid orderId, CancellationToken cancellationToken) =>
                {
                    await mediator.ExecuteCommandAsync(new PayForOrderCommand(orderId), cancellationToken);
                    return Results.NoContent();
                }).WithTags("Orders")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Orders.GetAllAdminOrders,
                async (int page, int pageSize, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var pagination = new PaginationParams(page, pageSize);

                    var result = await mediator.ExecuteQueryAsync<GetAdminOrdersQuery, PagedResult<OrderModel>>(
                        new GetAdminOrdersQuery(pagination),
                        cancellationToken);
                    return Results.Ok(result);
                }).WithTags("Orders")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.Orders.UpdateStatus,
                async (IMediator mediator, Guid orderId, [FromBody] Status status, CancellationToken cancellationToken) =>
                {
                    await mediator.ExecuteCommandAsync(new UpdateOrderStatusCommand(orderId, status), cancellationToken);
                    return Results.NoContent();
                }).WithTags("Orders")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}