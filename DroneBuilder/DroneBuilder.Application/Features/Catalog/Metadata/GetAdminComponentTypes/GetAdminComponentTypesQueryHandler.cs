using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.GetAdminComponentTypes;

public sealed class GetAdminComponentTypesQueryHandler(ICatalogMetadataRepository repository)
    : IQueryHandler<GetAdminComponentTypesQuery, ICollection<ComponentTypeModel>>
{
    public async Task<Result<ICollection<ComponentTypeModel>>> ExecuteAsync(
        GetAdminComponentTypesQuery query,
        CancellationToken cancellationToken)
    {
        ICollection<ComponentType> componentTypes =
            await repository.GetComponentTypesAsync(cancellationToken);

        return Result.Ok<ICollection<ComponentTypeModel>>(
            componentTypes.Select(componentType => componentType.ToModel()).ToList());
    }
}

public sealed record GetAdminComponentTypesQuery;
