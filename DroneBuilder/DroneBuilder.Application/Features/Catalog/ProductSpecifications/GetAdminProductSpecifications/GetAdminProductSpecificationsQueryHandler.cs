using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.GetAdminProductSpecifications;

public sealed class GetAdminProductSpecificationsQueryHandler(ICatalogSpecificationRepository repository)
    : IQueryHandler<GetAdminProductSpecificationsQuery, ICollection<ProductSpecificationModel>>
{
    public async Task<Result<ICollection<ProductSpecificationModel>>> ExecuteAsync(
        GetAdminProductSpecificationsQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(query.ProductId, cancellationToken);
        return product is null
            ? Result.Fail<ICollection<ProductSpecificationModel>>(new NotFoundError(
                $"Product with id {query.ProductId} not found."))
            : Result.Ok<ICollection<ProductSpecificationModel>>(product.ProductPropertyValues
                .OrderBy(specification => specification.Property!.Name)
                .ThenBy(specification => specification.Id)
                .Select(specification => specification.ToModel())
                .ToList());
    }
}

public sealed record GetAdminProductSpecificationsQuery(Guid ProductId);
