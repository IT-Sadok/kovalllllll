using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.PropertyQueries;

public class GetValuesByPropertyIdQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : IQueryHandler<GetValuesByPropertyIdQuery, PropertyResponseModel>
{
    public async Task<PropertyResponseModel> ExecuteAsync(GetValuesByPropertyIdQuery query,
        CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetValuesByPropertyIdAsync(query.PropertyId, cancellationToken);

        return mapper.Map<PropertyResponseModel>(property);
    }
}

public record GetValuesByPropertyIdQuery(Guid PropertyId);