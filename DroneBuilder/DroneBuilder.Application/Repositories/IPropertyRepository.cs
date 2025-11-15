using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IPropertyRepository
{
    Task AddPropertyAsync(Property property, CancellationToken cancellationToken = default);
    Task<Property?> GetPropertyByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ICollection<Property>> GetPropertiesAsync(CancellationToken cancellationToken = default);

    Task<Property> GetValuesByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default);

    void RemoveProperty(Property property);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}