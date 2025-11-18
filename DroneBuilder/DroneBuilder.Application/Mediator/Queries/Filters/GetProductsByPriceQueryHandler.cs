using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.Filters;

public class GetProductsByPriceQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetProductsByPriceQuery, ProductsResponseModel>
{
    public async Task<ProductsResponseModel> ExecuteAsync(GetProductsByPriceQuery query,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.GetByPriceAsync(query.MinPrice, query.MaxPrice, cancellationToken);

        if (products == null)
        {
            throw new NotFoundException($"No products found in the price range {query.MinPrice} - {query.MaxPrice}.");
        }

        return mapper.Map<ProductsResponseModel>(products);
    }
}

public record GetProductsByPriceQuery(decimal? MinPrice, decimal? MaxPrice);