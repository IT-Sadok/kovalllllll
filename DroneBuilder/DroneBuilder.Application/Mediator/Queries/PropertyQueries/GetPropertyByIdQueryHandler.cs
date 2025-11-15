using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.PropertyQueries;

public class GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : IQueryHandler<GetPropertyByIdQuery, PropertyResponseModel>
{
    public async Task<PropertyResponseModel> ExecuteAsync(GetPropertyByIdQuery query,
        CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetPropertyByIdAsync(query.PropertyId, cancellationToken);

        if (property is null)
        {
            throw new Exception($"Property with id {query.PropertyId} not found.");
        }

        return mapper.Map<PropertyResponseModel>(property);
    }
}

public record GetPropertyByIdQuery(Guid PropertyId);