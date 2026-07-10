using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.PropertyQueries;

public class GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : IQueryHandler<GetPropertyByIdQuery, PropertyModel>
{
    public async Task<PropertyModel> ExecuteAsync(GetPropertyByIdQuery query,
        CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetPropertyByIdAsync(query.PropertyId, cancellationToken);

        if (property is null)
        {
            throw new Exception($"Property with id {query.PropertyId} not found.");
        }

        return mapper.Map<PropertyModel>(property);
    }
}

public record GetPropertyByIdQuery(Guid PropertyId);
