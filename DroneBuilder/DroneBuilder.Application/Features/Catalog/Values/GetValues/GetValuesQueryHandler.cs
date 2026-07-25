using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Values.GetValues;

public class GetValuesQueryHandler(IValueRepository valueRepository)
    : IQueryHandler<GetValuesQuery, ICollection<ValueModel>>
{
    public async Task<Result<ICollection<ValueModel>>> ExecuteAsync(GetValuesQuery query, CancellationToken cancellationToken)
    {
        ICollection<Value> values = await valueRepository.GetValuesAsync(cancellationToken);

        if (values == null)
        {
            return Result.Fail<ICollection<ValueModel>>(new NotFoundError("Values not found."));
        }

        return Result.Ok<ICollection<ValueModel>>(values.Select(x => x.ToModel()).ToList());
    }
}

public record GetValuesQuery;
