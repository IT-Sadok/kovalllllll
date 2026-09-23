using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
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
