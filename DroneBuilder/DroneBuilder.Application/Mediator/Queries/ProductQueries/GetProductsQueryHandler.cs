using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetProductsQueryHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductModel>>
{
    public async Task<Result<PagedResult<ProductModel>>> ExecuteAsync(GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<Product>? products = await productRepository.GetFilteredPagedProductsAsync(
            query.Pagination,
            query.Filter,
            cancellationToken);

        if (products is null)
        {
            return Result.Fail<PagedResult<ProductModel>>(new NotFoundError("No products found."));
        }

        List<ProductModel> mappedItems = products.Items.Select(x => x.ToModel()).ToList();

        // Fetch stock levels from WarehouseRepository
        var productIds = mappedItems.Select(i => i.Id).ToList();
        ICollection<WarehouseItem> warehouseItems = await warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(productIds, cancellationToken);

        var stockMap = warehouseItems
            .GroupBy(wi => wi.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(wi => wi.Quantity));

        foreach (ProductModel item in mappedItems)
        {
            if (stockMap.TryGetValue(item.Id, out int quantity))
            {
                item.StockQuantity = quantity;
            }
        }

        return Result.Ok(new PagedResult<ProductModel>
        {
            Items = mappedItems,
            TotalCount = products.TotalCount,
            Page = products.Page,
            PageSize = products.PageSize
        });
    }
}

public record GetProductsQuery(PaginationParams Pagination, ProductFilterModel Filter);
