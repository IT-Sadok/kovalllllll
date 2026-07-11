using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ImageQueries;

public class GetImagesByProductIdQueryHandler(
    IProductRepository productRepository,
    IMapper mapper)
    : IQueryHandler<GetImagesByProductIdQuery, ICollection<ImageModel>>
{
    public async Task<Result<ICollection<ImageModel>>> ExecuteAsync(GetImagesByProductIdQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(query.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Fail<ICollection<ImageModel>>(new NotFoundError($"Product with id {query.ProductId} not found."));
        }

        return Result.Ok(mapper.Map<ICollection<ImageModel>>(product.Images));
    }
}

public record GetImagesByProductIdQuery(Guid ProductId);
