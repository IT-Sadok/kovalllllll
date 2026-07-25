using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications.GetProductSpecifications;

public sealed class GetProductSpecificationsQueryHandler(ICatalogSpecificationRepository repository)
    : IQueryHandler<GetProductSpecificationsQuery, ICollection<ProductSpecificationModel>>
{
    public async Task<Result<ICollection<ProductSpecificationModel>>> ExecuteAsync(
        GetProductSpecificationsQuery query,
        CancellationToken cancellationToken)
    {
        Product? product = await repository.GetProductAsync(query.ProductId, cancellationToken);
        if (product is null || product.PublicationStatus != ProductPublicationStatus.Published)
        {
            return Result.Fail<ICollection<ProductSpecificationModel>>(new NotFoundError(
                $"Product with id {query.ProductId} not found."));
        }

        return Result.Ok<ICollection<ProductSpecificationModel>>(
            product.ProductPropertyValues
                .OrderBy(specification => specification.Property!.Name)
                .ThenBy(specification => specification.Id)
                .Select(specification => specification.ToModel())
                .ToList());
    }
}

public sealed record GetProductSpecificationsQuery(Guid ProductId);
