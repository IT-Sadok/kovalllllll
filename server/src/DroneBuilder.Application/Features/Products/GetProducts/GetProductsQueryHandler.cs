using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Pagination;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Products.GetProducts;

public class GetProductsQueryHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductModel>>
{
    public async Task<Result<PagedResult<ProductModel>>> ExecuteAsync(GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<Product> products = await productRepository.GetFilteredPagedProductsAsync(
            query.Pagination,
            query.Filter,
            cancellationToken);

        List<ProductModel> mappedItems = products.Items.Select(x => x.ToModel()).ToList();

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

        List<Guid> groupIds = mappedItems.Where(i => i.GroupId.HasValue).Select(i => i.GroupId!.Value).Distinct().ToList();
        if (groupIds.Count > 0)
        {
            Dictionary<Guid, ProductGroupSummary> groups =
                (await productRepository.GetGroupSummariesAsync(groupIds, cancellationToken)).ToDictionary(g => g.Id);

            foreach (ProductModel item in mappedItems.Where(i => i.GroupId.HasValue))
            {
                if (groups.TryGetValue(item.GroupId!.Value, out ProductGroupSummary? group))
                {
                    item.Group = group.ToModel();
                }
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
