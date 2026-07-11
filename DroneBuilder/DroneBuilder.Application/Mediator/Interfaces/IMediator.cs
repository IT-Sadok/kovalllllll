using FluentResults;

namespace DroneBuilder.Application.Mediator.Interfaces;

public interface IMediator
{
    Task<Result> ExecuteCommandAsync<T>(T command, CancellationToken cancellationToken);
    Task<Result> ExecuteQueryAsync<T>(T query, CancellationToken cancellationToken);

    Task<Result<TResult>> ExecuteCommandAsync<T, TResult>(T command,
        CancellationToken cancellationToken);

    Task<Result<TResult>> ExecuteQueryAsync<T, TResult>(T query,
        CancellationToken cancellationToken);
}
