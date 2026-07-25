using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.GetComponentTypes;

public sealed class GetComponentTypesQueryHandler(ICatalogSpecificationRepository repository)
    : IQueryHandler<GetComponentTypesQuery, ICollection<ComponentTypeModel>>
{
    public async Task<Result<ICollection<ComponentTypeModel>>> ExecuteAsync(
        GetComponentTypesQuery query,
        CancellationToken cancellationToken)
    {
        ICollection<ComponentType> componentTypes =
            await repository.GetComponentTypesAsync(cancellationToken);

        return Result.Ok<ICollection<ComponentTypeModel>>(
            componentTypes.Select(componentType => componentType.ToModel()).ToList());
    }
}

public sealed record GetComponentTypesQuery;
