using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Products.GetManufacturers;

public class GetManufacturersQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetManufacturersQuery, ICollection<string>>
{
    public async Task<Result<ICollection<string>>> ExecuteAsync(GetManufacturersQuery query,
        CancellationToken cancellationToken)
    {
        return Result.Ok(await productRepository.GetManufacturersAsync(query.Category, cancellationToken));
    }
}

public record GetManufacturersQuery(ProductCategory? Category);
