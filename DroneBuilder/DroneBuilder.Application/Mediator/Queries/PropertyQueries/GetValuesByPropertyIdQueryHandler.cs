using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Queries.PropertyQueries;

public class GetValuesByPropertyIdQueryHandler(IPropertyRepository propertyRepository)
    : IQueryHandler<GetValuesByPropertyIdQuery, PropertyModel>
{
    public async Task<Result<PropertyModel>> ExecuteAsync(GetValuesByPropertyIdQuery query,
        CancellationToken cancellationToken)
    {
        Property property = await propertyRepository.GetValuesByPropertyIdAsync(query.PropertyId, cancellationToken);

        return Result.Ok(property.ToModel());
    }
}

public record GetValuesByPropertyIdQuery(Guid PropertyId);
