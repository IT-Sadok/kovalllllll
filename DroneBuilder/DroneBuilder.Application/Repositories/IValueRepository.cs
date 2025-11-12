using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IValueRepository
{
    Task AddValueAsync(Value value, CancellationToken cancellationToken = default);
    Task GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Value> GetValueAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Value>> GetValuesAsync(CancellationToken cancellationToken = default);
    Task RemoveValueAsync(Value value, CancellationToken cancellationToken = default);
    Task UpdateValueAsync(Value value, CancellationToken cancellationToken = default);

    Task<IEnumerable<Value>> GetValuesByPropertyIdAsync(Guid propertyId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}