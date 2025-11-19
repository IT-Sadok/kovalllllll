using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ValueQueries;

public class GetValuesQueryHandler(IValueRepository valueRepository, IMapper mapper)
    : IQueryHandler<GetValuesQuery, ICollection<ValueModel>>
{
    public async Task<ICollection<ValueModel>> ExecuteAsync(GetValuesQuery query, CancellationToken cancellationToken)
    {
        var values = await valueRepository.GetValuesAsync(cancellationToken);

        if (values == null)
        {
            throw new NotFoundException("Values not found.");
        }

        return mapper.Map<ICollection<ValueModel>>(values);
    }
}

public record GetValuesQuery;