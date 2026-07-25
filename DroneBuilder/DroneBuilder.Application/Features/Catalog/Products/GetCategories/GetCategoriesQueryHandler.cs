using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Products.GetCategories;

public class GetCategoriesQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetCategoriesQuery, IEnumerable<string>>
{
    public async Task<Result<IEnumerable<string>>> ExecuteAsync(GetCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        return Result.Ok(await productRepository.GetCategoriesAsync(cancellationToken));
    }
}

public record GetCategoriesQuery();
