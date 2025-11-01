namespace DroneBuilder.Application.Mediator.Interfaces;

public interface IQueryHandler<in T>
{
    Task ExecuteAsync(T query, CancellationToken cancellationToken);
}

public interface IQueryHandler<in T, TResult>
{
    Task<TResult> ExecuteAsync(T query, CancellationToken cancellationToken);
}