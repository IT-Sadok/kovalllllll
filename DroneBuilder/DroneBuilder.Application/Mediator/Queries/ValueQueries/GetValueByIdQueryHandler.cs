using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ValueQueries;

public class GetValueByIdQueryHandler(IValueRepository valueRepository, IMapper mapper)
    : IQueryHandler<GetValueByIdQuery, ValueResponseModel>
{
    public async Task<ValueResponseModel> ExecuteAsync(GetValueByIdQuery query, CancellationToken cancellationToken)
    {
        var value = await valueRepository.GetValueByIdAsync(query.PropertyId, cancellationToken);
        
        if (value == null)
        {
            throw new NotFoundException($"Value with id {query.PropertyId} not found.");
        }

        return mapper.Map<ValueResponseModel>(value);
    }
}

public record GetValueByIdQuery(Guid PropertyId);