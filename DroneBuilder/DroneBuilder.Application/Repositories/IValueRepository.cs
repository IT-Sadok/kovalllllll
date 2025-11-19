using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IValueRepository
{
    Task AddValueAsync(Value value, CancellationToken cancellationToken = default);
    Task<Value?> GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ICollection<Value>> GetValuesAsync(CancellationToken cancellationToken = default);
    void RemoveValue(Value value);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}