using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.ValueQueries;

public class GetValueByIdQueryHandler(IValueRepository valueRepository, IMapper mapper)
    : IQueryHandler<GetValueByIdQuery, ValueModel>
{
    public async Task<Result<ValueModel>> ExecuteAsync(GetValueByIdQuery query, CancellationToken cancellationToken)
    {
        Value? value = await valueRepository.GetValueByIdAsync(query.PropertyId, cancellationToken);

        if (value == null)
        {
            return Result.Fail<ValueModel>(new NotFoundError($"Value with id {query.PropertyId} not found."));
        }

        return Result.Ok(mapper.Map<ValueModel>(value));
    }
}

public record GetValueByIdQuery(Guid PropertyId);
