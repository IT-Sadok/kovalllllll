namespace DroneBuilder.Application.Common.Abstractions;

public interface IUnitOfWork
{
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
