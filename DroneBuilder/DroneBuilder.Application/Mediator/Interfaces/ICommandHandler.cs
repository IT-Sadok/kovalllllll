namespace DroneBuilder.Application.Mediator.Interfaces;

public interface ICommandHandler<in T>
{
    Task ExecuteCommandAsync(T command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in T, TResult>
{
    Task<TResult> ExecuteCommandAsync(T command, CancellationToken cancellationToken);
}