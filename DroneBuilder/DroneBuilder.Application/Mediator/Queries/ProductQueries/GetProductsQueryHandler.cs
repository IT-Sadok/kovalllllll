using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetProductsQuery, ProductsResponseModel>
{
    public async Task<ProductsResponseModel> ExecuteAsync(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetProductsAsync(cancellationToken);

        if (products is null)
        {
            throw new NotFoundException("No products found.");
        }

        return mapper.Map<ProductsResponseModel>(products);
    }
}

public record GetProductsQuery();