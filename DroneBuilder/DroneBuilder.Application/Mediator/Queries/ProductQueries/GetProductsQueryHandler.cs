using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
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

        return new PagedResult<ProductModel>
        {
            Items = mapper.Map<IEnumerable<ProductModel>>(products.Items),
            TotalCount = products.TotalCount,
            Page = products.Page,
            PageSize = products.PageSize
        };
    }
}

public record GetProductsQuery(PaginationParams Pagination, ProductFilterModel Filter);