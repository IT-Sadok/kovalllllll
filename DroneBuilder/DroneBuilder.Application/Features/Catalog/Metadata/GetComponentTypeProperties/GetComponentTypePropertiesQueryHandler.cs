using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.GetComponentTypeProperties;

public sealed class GetComponentTypePropertiesQueryHandler(ICatalogSpecificationRepository repository)
    : IQueryHandler<GetComponentTypePropertiesQuery, ComponentTypeDetailsModel>
{
    public async Task<Result<ComponentTypeDetailsModel>> ExecuteAsync(
        GetComponentTypePropertiesQuery query,
        CancellationToken cancellationToken)
    {
        ComponentType? componentType =
            await repository.GetComponentTypeAsync(query.ComponentTypeId, cancellationToken);

        return componentType is null
            ? Result.Fail<ComponentTypeDetailsModel>(new NotFoundError(
                $"Component type with id {query.ComponentTypeId} not found."))
            : Result.Ok(componentType.ToDetailsModel());
    }
}

public sealed record GetComponentTypePropertiesQuery(Guid ComponentTypeId);
