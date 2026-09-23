using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Models;
using DroneBuilder.Application.Common.Repositories;
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
