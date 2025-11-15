using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ValueQueries;

public class GetValueByIdQueryHandler(IValueRepository valueRepository, IMapper mapper)
    : IQueryHandler<GetValueByIdQuery>
{
    public async Task ExecuteAsync(GetValueByIdQuery query, CancellationToken cancellationToken)
    {
        var value = await valueRepository.GetValueByIdAsync(query.PropertyId, cancellationToken);
        if (value is null)
        {
            throw new KeyNotFoundException($"Value with PropertyId {query.PropertyId} not found.");
        }
    }
}

public record GetValueByIdQuery(Guid PropertyId);