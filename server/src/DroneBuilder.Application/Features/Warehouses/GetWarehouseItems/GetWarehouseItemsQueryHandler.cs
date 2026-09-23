using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Models;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Warehouses.GetWarehouseItems;

public class GetWarehouseItemsQueryHandler(IWarehouseRepository warehouseRepository)
    : IQueryHandler<GetWarehouseItemsQuery, PagedResult<WarehouseItemModel>>
{
    public async Task<Result<PagedResult<WarehouseItemModel>>> ExecuteAsync(GetWarehouseItemsQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<WarehouseItem> warehouseItems = await warehouseRepository.GetWarehouseItemsAsync(
            query.Pagination,
            cancellationToken);

        return Result.Ok(new PagedResult<WarehouseItemModel>
        {
            Items = warehouseItems.Items.Select(i => i.ToModel()),
            TotalCount = warehouseItems.TotalCount,
            Page = warehouseItems.Page,
            PageSize = warehouseItems.PageSize
        });
    }
}

public record GetWarehouseItemsQuery(PaginationParams Pagination);
