using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Properties.GetPropertyById;

public class GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository)
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

        return Result.Ok(property.ToModel());
    }
}

public record GetPropertyByIdQuery(Guid PropertyId);
