using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductsQuery, ProductsResponseModel>
{
    public async Task<ProductsResponseModel> ExecuteAsync(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetProductsAsync(cancellationToken);

        var response = new ProductsResponseModel
        {
            Products = products.Select(product => new ProductResponseModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Images = product.Images,
                Properties = product.Properties
            }).ToList()
        };

        return response;
    }
}

public record GetProductsQuery();