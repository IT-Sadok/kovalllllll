using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.PropertyQueries;

public class GetPropertiesQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : IQueryHandler<GetPropertiesQuery, ICollection<PropertyModel>>
{
    public async Task<Result<ICollection<PropertyModel>>> ExecuteAsync(GetPropertiesQuery query,
        CancellationToken cancellationToken)
    {
        ICollection<Property>? properties = await propertyRepository.GetPropertiesAsync(cancellationToken);

        if (properties is null)
        {
            return Result.Fail<ICollection<PropertyModel>>(new NotFoundError("No properties found."));
        }

        return Result.Ok(mapper.Map<ICollection<PropertyModel>>(properties));
    }
}

public record GetPropertiesQuery;
