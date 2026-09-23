using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Products.GetDelistedProducts;

public class GetDelistedProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetDelistedProductsQuery, PagedResult<ProductModel>>
{
    public async Task<Result<PagedResult<ProductModel>>> ExecuteAsync(GetDelistedProductsQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<Product> products =
            await productRepository.GetDelistedProductsAsync(query.Pagination, cancellationToken);

        return Result.Ok(new PagedResult<ProductModel>
        {
            Items = products.Items.Select(p => p.ToModel()).ToList(),
            TotalCount = products.TotalCount,
            Page = products.Page,
            PageSize = products.PageSize
        });
    }
}

public record GetDelistedProductsQuery(PaginationParams Pagination);
