using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using FluentResults;

namespace DroneBuilder.Application.Features.Products.GetCategories;

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
