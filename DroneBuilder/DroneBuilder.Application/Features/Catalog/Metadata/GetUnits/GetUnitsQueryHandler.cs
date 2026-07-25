using DroneBuilder.Application.Features.Catalog.Metadata.Models;
using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Metadata.GetUnits;

public sealed class GetUnitsQueryHandler(ICatalogSpecificationRepository repository)
    : IQueryHandler<GetUnitsQuery, ICollection<UnitDefinitionModel>>
{
    public async Task<Result<ICollection<UnitDefinitionModel>>> ExecuteAsync(
        GetUnitsQuery query,
        CancellationToken cancellationToken)
    {
        ICollection<UnitDefinition> units = await repository.GetUnitsAsync(cancellationToken);
        return Result.Ok<ICollection<UnitDefinitionModel>>(
            units.Select(unit => unit.ToModel()).ToList());
    }
}

public sealed record GetUnitsQuery;
