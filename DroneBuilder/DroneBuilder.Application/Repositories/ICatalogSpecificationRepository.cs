using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface ICatalogSpecificationRepository
{
    Task<ICollection<ComponentType>> GetComponentTypesAsync(CancellationToken cancellationToken = default);
    Task<ComponentType?> GetComponentTypeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ICollection<UnitDefinition>> GetUnitsAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Property?> GetPropertyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsSkuInUseAsync(string sku, Guid? excludedVariantId = null, CancellationToken cancellationToken = default);
    void RemoveSpecification(ProductPropertyValue specification);
    void RemoveVariantSpecification(ProductVariantPropertyValue specification);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
