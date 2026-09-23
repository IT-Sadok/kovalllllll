using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Warehouses.GetWarehouseItemById;

public class GetWarehouseItemByIdQueryHandler(IWarehouseRepository warehouseRepository)
    : IQueryHandler<GetWarehouseItemByIdQuery, WarehouseItemModel>
{
    public async Task<Result<WarehouseItemModel>> ExecuteAsync(GetWarehouseItemByIdQuery query,
        CancellationToken cancellationToken)
    {
        WarehouseItem? warehouseItem =
            await warehouseRepository.GetWarehouseItemByIdAsync(query.WarehouseItemId, cancellationToken);
        if (warehouseItem == null)
        {
            return Result.Fail<WarehouseItemModel>(new NotFoundError($"Warehouse item with id {query.WarehouseItemId} not found."));
        }

        return Result.Ok(warehouseItem.ToModel());
    }
}

public record GetWarehouseItemByIdQuery(Guid WarehouseItemId);
