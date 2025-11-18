using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.Filters;

public class GetProductsByNameQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetProductsByNameQuery, ProductsResponseModel>
{
    public async Task<ProductsResponseModel> ExecuteAsync(GetProductsByNameQuery query,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.GetByNameAsync(query.Name, cancellationToken);

        if (products == null)
        {
            throw new NotFoundException($"Products with name '{query.Name}' were not found.");
        }

        var productsResponse = mapper.Map<ProductsResponseModel>(products);
        return productsResponse;
    }
}

public record GetProductsByNameQuery(string Name);