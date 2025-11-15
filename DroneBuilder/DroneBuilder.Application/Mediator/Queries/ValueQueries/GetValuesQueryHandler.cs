using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ValueQueries;

public class GetValuesQueryHandler(IValueRepository valueRepository, IMapper mapper)
    : IQueryHandler<GetValuesQuery, ValuesResponseModel>
{
    public async Task<ValuesResponseModel> ExecuteAsync(GetValuesQuery query, CancellationToken cancellationToken)
    {
        var values = await valueRepository.GetValuesAsync(cancellationToken);

        if (values == null)
        {
            throw new NotFoundException("Values not found.");
        }

        return mapper.Map<ValuesResponseModel>(values);
    }
}

public record GetValuesQuery;