using FluentResults;

namespace DroneBuilder.Application.Mediator.Interfaces;

public interface IQueryHandler<in T>
{
    Task<Result> ExecuteAsync(T query, CancellationToken cancellationToken);
}

public interface IQueryHandler<in T, TResult>
{
    Task<Result<TResult>> ExecuteAsync(T query, CancellationToken cancellationToken);
}
