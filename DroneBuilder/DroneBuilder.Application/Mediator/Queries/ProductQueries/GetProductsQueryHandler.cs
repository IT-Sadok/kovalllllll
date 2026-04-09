using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetProductsQueryHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IMapper mapper)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductModel>>
{
    public async Task<PagedResult<ProductModel>> ExecuteAsync(GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.GetFilteredPagedProductsAsync(
            query.Pagination,
            query.Filter,
            cancellationToken);

        if (products is null)
        {
            throw new NotFoundException("No products found.");
        }

        var mappedItems = mapper.Map<List<ProductModel>>(products.Items);

        // Fetch stock levels from WarehouseRepository
        var productIds = mappedItems.Select(i => i.Id).ToList();
        var warehouseItems = await warehouseRepository.GetAllWarehouseItemsByProductIdsAsync(productIds, cancellationToken);
        
        var stockMap = warehouseItems
            .GroupBy(wi => wi.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(wi => wi.Quantity));

        foreach (var item in mappedItems)
        {
            if (stockMap.TryGetValue(item.Id, out var quantity))
            {
                item.StockQuantity = quantity;
            }
        }

        return new PagedResult<ProductModel>
        {
            Items = mappedItems,
            TotalCount = products.TotalCount,
            Page = products.Page,
            PageSize = products.PageSize
        };
    }
}

public record GetProductsQuery(PaginationParams Pagination, ProductFilterModel Filter);