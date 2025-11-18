using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ImageQueries;

public class GetImagesByProductIdQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetImagesByProductIdQuery, ProductImagesResponseModel>
{
    public async Task<ProductImagesResponseModel> ExecuteAsync(GetImagesByProductIdQuery query,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product with id {query.ProductId} not found.");
        }

        return mapper.Map<ProductImagesResponseModel>(product.Images);
    }
}

public record GetImagesByProductIdQuery(Guid ProductId);