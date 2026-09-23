using FluentResults;

namespace DroneBuilder.Application.Common.Mediator.Interfaces;

public interface ICommandHandler<in T>
{
    Task<Result> ExecuteCommandAsync(T command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in T, TResult>
{
    Task<Result<TResult>> ExecuteCommandAsync(T command, CancellationToken cancellationToken);
}
