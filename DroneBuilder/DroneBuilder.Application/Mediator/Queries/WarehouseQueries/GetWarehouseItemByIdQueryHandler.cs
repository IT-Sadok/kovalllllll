using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.WarehouseQueries;

public class GetWarehouseItemByIdQueryHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
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

        return Result.Ok(mapper.Map<WarehouseItemModel>(warehouseItem));
    }
}

public record GetWarehouseItemByIdQuery(Guid WarehouseItemId);
