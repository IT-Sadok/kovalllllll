using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Queries.WarehouseQueries;

public class GetWarehouseQueryHandler(IWarehouseRepository warehouseRepository)
    : IQueryHandler<GetWarehouseQuery, WarehouseModel>
{
    public async Task<Result<WarehouseModel>> ExecuteAsync(GetWarehouseQuery query, CancellationToken cancellationToken)
    {
        Warehouse? warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken);
        if (warehouse == null)
        {
            return Result.Fail<WarehouseModel>(new NotFoundError("Warehouse not found."));
        }

        return Result.Ok(warehouse.ToModel());
    }
}

public record GetWarehouseQuery;
