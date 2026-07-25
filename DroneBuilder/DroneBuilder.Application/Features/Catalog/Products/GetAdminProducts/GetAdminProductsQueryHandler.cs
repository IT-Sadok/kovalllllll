using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Products.GetAdminProducts;

public sealed class GetAdminProductsQueryHandler(IProductRepository repository)
    : IQueryHandler<GetAdminProductsQuery, PagedResult<ProductModel>>
{
    public async Task<Result<PagedResult<ProductModel>>> ExecuteAsync(
        GetAdminProductsQuery query,
        CancellationToken cancellationToken)
    {
        PagedResult<Product> products = await repository.GetAdminProductsAsync(
            query.Pagination,
            query.Filter,
            cancellationToken);
        return Result.Ok(new PagedResult<ProductModel>
        {
            Items = products.Items.Select(product => product.ToModel()).ToList(),
            TotalCount = products.TotalCount,
            Page = products.Page,
            PageSize = products.PageSize
        });
    }
}

public sealed record GetAdminProductsQuery(
    PaginationParams Pagination,
    AdminProductFilterModel Filter);
