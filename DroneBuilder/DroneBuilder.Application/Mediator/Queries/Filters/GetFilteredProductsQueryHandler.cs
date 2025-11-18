using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.Filters;

public class GetFilteredProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetFilteredProductsQuery, ProductsResponseModel>
{
    public async Task<ProductsResponseModel> ExecuteAsync(GetFilteredProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.GetByFilterAsync(query.Filter, cancellationToken);

        return mapper.Map<ProductsResponseModel>(products);
    }
}

public record GetFilteredProductsQuery(ProductFilterModel Filter);