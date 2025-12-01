using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Commands.WarehouseCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.WarehouseQueries;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.WarehouseModels;
using Microsoft.AspNetCore.Mvc;

namespace DroneBuilder.API.Endpoints;

public static class WarehouseEndpointExtensions
{
    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Warehouses.Get,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetWarehouseQuery, WarehouseModel>(
                        new GetWarehouseQuery(),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .WithTags("Warehouse")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Warehouses.GetItemById,
                async (IMediator mediator, Guid warehouseItemId, CancellationToken cancellationToken) =>
                {
                    var result = await mediator.ExecuteQueryAsync<GetWarehouseItemByIdQuery, WarehouseItemModel>(
                        new GetWarehouseItemByIdQuery(warehouseItemId),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .WithTags("Warehouse")
            .RequireAuthorization();

        app.MapPost(ApiRoutes.Warehouses.AddQuantityToItem, async (IMediator mediator, Guid warehouseItemId,
                [FromBody] AddQuantityModel model,
                CancellationToken cancellationToken) =>
            {
                var result =
                    await mediator.ExecuteCommandAsync<AddQuantityToWarehouseItemCommand, WarehouseItemModel>(
                        new AddQuantityToWarehouseItemCommand(warehouseItemId, model),
                        cancellationToken);
                return Results.Ok(result);
            })
            .WithTags("Warehouse")
            .RequireAuthorization();

        app.MapDelete(ApiRoutes.Warehouses.RemoveQuantityFromItem, async (IMediator mediator, Guid warehouseItemId,
                [FromBody] RemoveQuantityModel model,
                CancellationToken cancellationToken) =>
            {
                var result =
                    await mediator.ExecuteCommandAsync<RemoveQuantityFromWarehouseItemCommand, WarehouseItemModel>(
                        new RemoveQuantityFromWarehouseItemCommand(warehouseItemId, model),
                        cancellationToken);
                return Results.Ok(result);
            })
            .WithTags("Warehouse")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Warehouses.GetAllItems,
                async (int page, int pageSize, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var pagination = new PaginationParams(page, pageSize);
                    var result =
                        await mediator.ExecuteQueryAsync<GetWarehouseItemsQuery, PagedResult<WarehouseItemModel>>(
                            new GetWarehouseItemsQuery(pagination),
                            cancellationToken);
                    return Results.Ok(result);
                })
            .WithTags("Warehouse")
            .RequireAuthorization();


        return app;
    }
}