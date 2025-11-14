using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetPropertiesByProductIdQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetPropertiesByProductIdQuery, ProductPropertiesResponseModel>
{
    public async Task<ProductPropertiesResponseModel> ExecuteAsync(GetPropertiesByProductIdQuery query,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with id {query.ProductId} not found.");
        }

        return new ProductPropertiesResponseModel
        {
            Id = product.Id,
            Name = product.Name,
            Properties = product.Properties
        };
    }
}

public record GetPropertiesByProductIdQuery(Guid ProductId);