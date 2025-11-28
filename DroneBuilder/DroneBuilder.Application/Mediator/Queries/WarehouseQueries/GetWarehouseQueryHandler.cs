using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.WarehouseQueries;

public class GetWarehouseQueryHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
    : IQueryHandler<GetWarehouseQuery, WarehouseModel>
{
    public async Task<WarehouseModel> ExecuteAsync(GetWarehouseQuery query, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken);
        if (warehouse == null)
        {
            throw new NotFoundException("Warehouse not found.");
        }

        return mapper.Map<WarehouseModel>(warehouse);
    }
}

public record GetWarehouseQuery;