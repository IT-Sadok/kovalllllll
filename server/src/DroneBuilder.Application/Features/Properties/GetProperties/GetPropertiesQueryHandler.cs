using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Properties.GetProperties;

public class GetPropertiesQueryHandler(IPropertyRepository propertyRepository)
    : IQueryHandler<GetPropertiesQuery, ICollection<PropertyModel>>
{
    public async Task<Result<ICollection<PropertyModel>>> ExecuteAsync(GetPropertiesQuery query,
        CancellationToken cancellationToken)
    {
        ICollection<Property> properties = await propertyRepository.GetPropertiesAsync(cancellationToken);

        return Result.Ok<ICollection<PropertyModel>>(properties.Select(x => x.ToModel()).ToList());
    }
}

public record GetPropertiesQuery;
