using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.Filters;

public class GetProductsByCategoryQueryHandler(IMapper mapper, IProductRepository productRepository)
    : IQueryHandler<GetProductsByCategoryQuery, ProductsResponseModel>
{
    public async Task<ProductsResponseModel> ExecuteAsync(GetProductsByCategoryQuery query,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.GetByCategoryAsync(query.CategoryName, cancellationToken);

        if (products == null)
        {
            throw new NotFoundException($"No products found in category: {query.CategoryName}");
        }

        return mapper.Map<ProductsResponseModel>(products);
    }
}

public record GetProductsByCategoryQuery(string CategoryName);