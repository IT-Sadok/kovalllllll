using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Queries.ProductQueries;

public class GetCategoriesQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetCategoriesQuery, IEnumerable<string>>
{
    public async Task<IEnumerable<string>> ExecuteAsync(GetCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        return await productRepository.GetCategoriesAsync(cancellationToken);
    }
}

public record GetCategoriesQuery();
