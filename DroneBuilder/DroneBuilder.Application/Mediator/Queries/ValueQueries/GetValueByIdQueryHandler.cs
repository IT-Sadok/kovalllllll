using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ValueQueries;

public class GetValueByIdQueryHandler(IValueRepository valueRepository, IMapper mapper)
    : IQueryHandler<GetValueByIdQuery, ValueModel>
{
    public async Task<ValueModel> ExecuteAsync(GetValueByIdQuery query, CancellationToken cancellationToken)
    {
        var value = await valueRepository.GetValueByIdAsync(query.PropertyId, cancellationToken);
        
        if (value == null)
        {
            throw new NotFoundException($"Value with id {query.PropertyId} not found.");
        }

        return mapper.Map<ValueModel>(value);
    }
}

public record GetValueByIdQuery(Guid PropertyId);