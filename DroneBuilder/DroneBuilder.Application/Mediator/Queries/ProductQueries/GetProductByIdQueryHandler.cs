using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetProductByIdQuery, ProductModel>
{
    public async Task<ProductModel> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with id {query.ProductId} not found.");
        }

        return mapper.Map<ProductModel>(product);
    }
}

public record GetProductByIdQuery(Guid ProductId);