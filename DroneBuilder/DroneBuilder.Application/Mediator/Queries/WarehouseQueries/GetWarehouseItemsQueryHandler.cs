using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.WarehouseQueries;

public class GetWarehouseItemsQueryHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
    : IQueryHandler<GetWarehouseItemsQuery, PagedResult<WarehouseItemModel>>
{
    public async Task<PagedResult<WarehouseItemModel>> ExecuteAsync(GetWarehouseItemsQuery query,
        CancellationToken cancellationToken)
    {
        var warehouseItems = await warehouseRepository.GetWarehouseItemsAsync(
            query.Pagination,
            cancellationToken);

        if (warehouseItems is null)
        {
            throw new NotFoundException("No warehouse items found.");
        }

        return new PagedResult<WarehouseItemModel>
        {
            Items = mapper.Map<IEnumerable<WarehouseItemModel>>(warehouseItems.Items),
            TotalCount = warehouseItems.TotalCount,
            Page = warehouseItems.Page,
            PageSize = warehouseItems.PageSize
        };
    }
}

public record GetWarehouseItemsQuery(PaginationParams Pagination);