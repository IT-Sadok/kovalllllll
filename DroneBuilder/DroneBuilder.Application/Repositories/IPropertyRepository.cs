using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IPropertyRepository
{
    Task AddPropertyAsync(Property property, CancellationToken cancellationToken = default);
    Task GetPropertyByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Property> GetPropertyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Property>> GetPropertiesAsync(CancellationToken cancellationToken = default);
    Task RemovePropertyAsync(Property property, CancellationToken cancellationToken = default);
    Task UpdatePropertyAsync(Property property, CancellationToken cancellationToken = default);
    Task GetPropertiesByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task GetPropertyValuesAsync(Property property, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}