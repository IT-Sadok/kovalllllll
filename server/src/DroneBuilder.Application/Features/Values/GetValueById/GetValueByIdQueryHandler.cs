using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Values.GetValueById;

public class GetValueByIdQueryHandler(IValueRepository valueRepository)
    : IQueryHandler<GetValueByIdQuery, ValueModel>
{
    public async Task<Result<ValueModel>> ExecuteAsync(GetValueByIdQuery query, CancellationToken cancellationToken)
    {
        Value? value = await valueRepository.GetValueByIdAsync(query.ValueId, cancellationToken);

        if (value == null)
        {
            return Result.Fail<ValueModel>(new NotFoundError($"Value with id {query.ValueId} not found."));
        }

        return Result.Ok(value.ToModel());
    }
}

public record GetValueByIdQuery(Guid ValueId);
