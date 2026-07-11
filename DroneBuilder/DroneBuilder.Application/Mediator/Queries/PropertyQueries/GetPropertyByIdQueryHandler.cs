using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.PropertyQueries;

public class GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository, IMapper mapper)
    : IQueryHandler<GetPropertyByIdQuery, PropertyModel>
{
    public async Task<Result<PropertyModel>> ExecuteAsync(GetPropertyByIdQuery query,
        CancellationToken cancellationToken)
    {
        Property? property = await propertyRepository.GetPropertyByIdAsync(query.PropertyId, cancellationToken);

        if (property is null)
        {
            return Result.Fail<PropertyModel>(new NotFoundError($"Property with id {query.PropertyId} not found."));
        }

        return Result.Ok(mapper.Map<PropertyModel>(property));
    }
}

public record GetPropertyByIdQuery(Guid PropertyId);
