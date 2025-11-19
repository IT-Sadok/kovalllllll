using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetPropertiesByProductIdQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetPropertiesByProductIdQuery, ProductPropertiesResponseModel>
{
    public async Task<ProductPropertiesResponseModel> ExecuteAsync(GetPropertiesByProductIdQuery query,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetPropertiesByProductIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with id {query.ProductId} not found.");
        }

        return mapper.Map<ProductPropertiesResponseModel>(product);
    }
}

public record GetPropertiesByProductIdQuery(Guid ProductId);