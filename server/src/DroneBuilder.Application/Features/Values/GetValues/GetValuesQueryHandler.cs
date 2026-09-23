using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Values.GetValues;

public class GetValuesQueryHandler(IValueRepository valueRepository)
    : IQueryHandler<GetValuesQuery, ICollection<ValueModel>>
{
    public async Task<Result<ICollection<ValueModel>>> ExecuteAsync(GetValuesQuery query, CancellationToken cancellationToken)
    {
        ICollection<Value> values = await valueRepository.GetValuesAsync(cancellationToken);

        return Result.Ok<ICollection<ValueModel>>(values.Select(x => x.ToModel()).ToList());
    }
}

public record GetValuesQuery;
