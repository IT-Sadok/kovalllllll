using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Properties.GetValuesByPropertyId;

public class GetValuesByPropertyIdQueryHandler(IPropertyRepository propertyRepository)
    : IQueryHandler<GetValuesByPropertyIdQuery, PropertyModel>
{
    public async Task<Result<PropertyModel>> ExecuteAsync(GetValuesByPropertyIdQuery query,
        CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetValuesByPropertyIdAsync(query.PropertyId, cancellationToken);
        if (property is null)
        {
            return Result.Fail<PropertyModel>(new DroneBuilder.Application.ResultErrors.NotFoundError(
                $"Property with id {query.PropertyId} not found."));
        }

        return Result.Ok(property.ToModel());
    }
}

public record GetValuesByPropertyIdQuery(Guid PropertyId);
