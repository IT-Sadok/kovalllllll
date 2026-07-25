using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Features.Orders.CreateOrder;
using DroneBuilder.Application.Features.Orders.GetAdminOrders;
using DroneBuilder.Application.Features.Orders.GetOrders;
using DroneBuilder.Application.Features.Orders.PayForOrder;
using DroneBuilder.Application.Features.Orders.UpdateOrderStatus;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace DroneBuilder.API.Features.Orders;

public static class OrderEndpointExtensions
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Orders.CreateOrder,
                async (IMediator mediator, ShippingDetailsModel shippingDetails,
                    CancellationToken cancellationToken) =>
                {
                    Result<OrderModel> result = await mediator.ExecuteCommandAsync<CreateOrderCommand, OrderModel>(
                        new CreateOrderCommand(shippingDetails),
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Orders")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Orders.GetAllOrders,
                async (int page, int pageSize, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var pagination = new PaginationParams(page, pageSize);

                    Result<PagedResult<OrderModel>> result = await mediator.ExecuteQueryAsync<GetOrdersQuery, PagedResult<OrderModel>>(
                        new GetOrdersQuery(pagination),
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Orders")
            .RequireAuthorization();

        app.MapPatch(ApiRoutes.Orders.PayForOrder,
                async (IMediator mediator, Guid orderId, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(new PayForOrderCommand(orderId), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Orders")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Orders.GetAllAdminOrders,
                async (int page, int pageSize, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var pagination = new PaginationParams(page, pageSize);

                    Result<PagedResult<OrderModel>> result = await mediator.ExecuteQueryAsync<GetAdminOrdersQuery, PagedResult<OrderModel>>(
                        new GetAdminOrdersQuery(pagination),
                        cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Orders")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.Orders.UpdateStatus,
                async (IMediator mediator, Guid orderId, [FromBody] Status status, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(new UpdateOrderStatusCommand(orderId, status), cancellationToken);
                    return result.ToHttpResult();
                }).WithTags("Orders")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}
