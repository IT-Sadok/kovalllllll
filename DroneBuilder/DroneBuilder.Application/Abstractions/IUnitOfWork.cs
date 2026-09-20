namespace DroneBuilder.Application.Abstractions;

/// <summary>
/// Lets a handler make several repository operations atomic. Only useful because every repository
/// in a request shares one database context.
/// </summary>
public interface IUnitOfWork
{
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>Rolls back when disposed without a commit.</summary>
public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
