namespace DroneBuilder.Application.Mediator.Interfaces;

public interface IMediator
{
    Task ExecuteCommandAsync<T>(T command, CancellationToken cancellationToken);
    Task ExecuteQueryAsync<T>(T query, CancellationToken cancellationToken);

    Task<TResult> ExecuteCommandAsync<T, TResult>(T command,
        CancellationToken cancellationToken);

    Task<TResult> ExecuteQueryAsync<T, TResult>(T query,
        CancellationToken cancellationToken);
}