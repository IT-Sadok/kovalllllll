using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.WarehouseQueries;

public class GetWarehouseItemByIdQueryHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
    : IQueryHandler<GetWarehouseItemByIdQuery, WarehouseItemModel>
{
    public async Task<WarehouseItemModel> ExecuteAsync(GetWarehouseItemByIdQuery query,
        CancellationToken cancellationToken)
    {
        var warehouseItem =
            await warehouseRepository.GetWarehouseItemByIdAsync(query.WarehouseItemId, cancellationToken);
        if (warehouseItem == null)
        {
            throw new Exception($"Warehouse item with id {query.WarehouseItemId} not found.");
        }

        return mapper.Map<WarehouseItemModel>(warehouseItem);
    }
}

public record GetWarehouseItemByIdQuery(Guid WarehouseItemId);