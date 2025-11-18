using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.PropertyQueries;

public class GetPropertiesQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : IQueryHandler<GetPropertiesQuery, ICollection<PropertyModel>>
{
    public async Task<ICollection<PropertyModel>> ExecuteAsync(GetPropertiesQuery query,
        CancellationToken cancellationToken)
    {
        var properties = await propertyRepository.GetPropertiesAsync(cancellationToken);

        if (properties == null)
        {
            throw new NotFoundException("No properties found.");
        }

        return mapper.Map<ICollection<PropertyModel>>(properties);
    }
}

public record GetPropertiesQuery;