using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ImageQueries;

public class GetImagesByProductIdQueryHandler(
    IProductRepository productRepository,
    IMapper mapper)
    : IQueryHandler<GetImagesByProductIdQuery, ICollection<ImageModel>>
{
    public async Task<ICollection<ImageModel>> ExecuteAsync(GetImagesByProductIdQuery query,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with id {query.ProductId} not found.");
        }

        return mapper.Map<ICollection<ImageModel>>(product.Images);
    }
}

public record GetImagesByProductIdQuery(Guid ProductId);