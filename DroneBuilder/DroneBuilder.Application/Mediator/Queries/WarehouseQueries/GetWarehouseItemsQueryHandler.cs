using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.WarehouseQueries;

public class GetWarehouseItemsQueryHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
    : IQueryHandler<GetWarehouseItemsQuery, PagedResult<WarehouseItemModel>>
{
    public async Task<Result<PagedResult<WarehouseItemModel>>> ExecuteAsync(GetWarehouseItemsQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<WarehouseItem>? warehouseItems = await warehouseRepository.GetWarehouseItemsAsync(
            query.Pagination,
            cancellationToken);

        if (warehouseItems is null)
        {
            return Result.Fail<PagedResult<WarehouseItemModel>>(new NotFoundError("No warehouse items found."));
        }

        return Result.Ok(new PagedResult<WarehouseItemModel>
        {
            Items = mapper.Map<IEnumerable<WarehouseItemModel>>(warehouseItems.Items),
            TotalCount = warehouseItems.TotalCount,
            Page = warehouseItems.Page,
            PageSize = warehouseItems.PageSize
        });
    }
}

public record GetWarehouseItemsQuery(PaginationParams Pagination);
