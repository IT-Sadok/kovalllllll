using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface ICatalogMetadataRepository
{
    Task<ICollection<ComponentType>> GetComponentTypesAsync(CancellationToken cancellationToken = default);
    Task<ComponentType?> GetComponentTypeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Property?> GetPropertyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsComponentTypeCodeInUseAsync(string code, Guid? excludedId = null, CancellationToken cancellationToken = default);
    Task AddComponentTypeAsync(ComponentType componentType, CancellationToken cancellationToken = default);
    void RemoveComponentTypeRule(ComponentTypeProperty rule);

    Task<UnitDefinition?> GetUnitAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsUnitCodeInUseAsync(string code, Guid? excludedId = null, CancellationToken cancellationToken = default);
    Task AddUnitAsync(UnitDefinition unit, CancellationToken cancellationToken = default);
    void RemoveUnit(UnitDefinition unit);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
