using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using DroneBuilder.Application.Mappings;
namespace DroneBuilder.Application.Mediator.Queries.WarehouseQueries;

public class GetWarehouseItemsQueryHandler(IWarehouseRepository warehouseRepository)
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
            Items = warehouseItems.Items.Select(i => i.ToModel()),
            TotalCount = warehouseItems.TotalCount,
            Page = warehouseItems.Page,
            PageSize = warehouseItems.PageSize
        });
    }
}

public record GetWarehouseItemsQuery(PaginationParams Pagination);
