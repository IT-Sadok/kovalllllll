using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Queries.ValueQueries;

public class GetValueByIdQueryHandler(IValueRepository valueRepository)
    : IQueryHandler<GetValueByIdQuery, ValueModel>
{
    public async Task<Result<ValueModel>> ExecuteAsync(GetValueByIdQuery query, CancellationToken cancellationToken)
    {
        Value? value = await valueRepository.GetValueByIdAsync(query.PropertyId, cancellationToken);

        if (value == null)
        {
            return Result.Fail<ValueModel>(new NotFoundError($"Value with id {query.PropertyId} not found."));
        }

        return Result.Ok(value.ToModel());
    }
}

public record GetValueByIdQuery(Guid PropertyId);
