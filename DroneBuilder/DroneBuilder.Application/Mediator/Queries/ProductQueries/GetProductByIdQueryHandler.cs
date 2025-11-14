using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductByIdQuery, ProductResponseModel>
{
    public async Task<ProductResponseModel> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with id {query.ProductId} not found.");
        }

        return new ProductResponseModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}

public record GetProductByIdQuery(Guid ProductId);