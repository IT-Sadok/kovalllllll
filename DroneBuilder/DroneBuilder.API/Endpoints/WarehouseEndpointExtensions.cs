using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Commands.WarehouseCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Mediator.Queries.WarehouseQueries;
using DroneBuilder.Application.Models.WarehouseModels;

namespace DroneBuilder.API.Endpoints;

public static class WarehouseEndpointExtensions
{
    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiRoutes.Warehouses.UpdateWarehouseItem, async (IMediator mediator, Guid warehouseItemId,
                UpdateWarehouseItemModel updateModel,
                CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteCommandAsync<UpdateWarehouseItemCommand, WarehouseItemModel>(
                    new UpdateWarehouseItemCommand(warehouseItemId, updateModel),
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithTags("Warehouse")
            .RequireAuthorization();

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

        return app;
    }
}