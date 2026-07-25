using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public sealed class CatalogSpecificationRepository(ApplicationDbContext dbContext)
    : ICatalogSpecificationRepository
{
    public async Task<ICollection<ComponentType>> GetComponentTypesAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ComponentTypes
            .AsNoTracking()
            .Where(componentType => componentType.IsActive)
            .OrderBy(componentType => componentType.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ComponentType?> GetComponentTypeAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ComponentTypes
            .AsSplitQuery()
            .Include(componentType => componentType.Properties)
                .ThenInclude(rule => rule.Property)
                    .ThenInclude(property => property!.UnitDefinition)
                        .ThenInclude(unit => unit!.Aliases)
            .Include(componentType => componentType.Properties)
                .ThenInclude(rule => rule.Property)
                    .ThenInclude(property => property!.Values)
            .FirstOrDefaultAsync(
                componentType => componentType.Id == id && componentType.IsActive,
                cancellationToken);
    }

    public async Task<ICollection<UnitDefinition>> GetUnitsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UnitDefinitions
            .AsNoTracking()
            .Include(unit => unit.Aliases)
            .OrderBy(unit => unit.Dimension)
            .ThenBy(unit => unit.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetProductAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Products
            .AsSplitQuery()
            .Include(product => product.ComponentType)
                .ThenInclude(componentType => componentType!.Properties)
            .Include(product => product.ProductPropertyValues)
                .ThenInclude(specification => specification.Property)
                    .ThenInclude(property => property!.UnitDefinition)
                        .ThenInclude(unit => unit!.Aliases)
            .Include(product => product.ProductPropertyValues)
                .ThenInclude(specification => specification.Property)
                    .ThenInclude(property => property!.Values)
            .Include(product => product.ProductPropertyValues)
                .ThenInclude(specification => specification.Value)
            .Include(product => product.Variants)
                .ThenInclude(variant => variant.Specifications)
                    .ThenInclude(specification => specification.Property)
                        .ThenInclude(property => property!.UnitDefinition)
                            .ThenInclude(unit => unit!.Aliases)
            .Include(product => product.Variants)
                .ThenInclude(variant => variant.Specifications)
                    .ThenInclude(specification => specification.Property)
                        .ThenInclude(property => property!.Values)
            .Include(product => product.Variants)
                .ThenInclude(variant => variant.Specifications)
                    .ThenInclude(specification => specification.Value)
            .Include(product => product.Variants)
                .ThenInclude(variant => variant.WarehouseItems)
            .FirstOrDefaultAsync(
                product => product.Id == id && product.IsActive,
                cancellationToken);
    }

    public async Task<Property?> GetPropertyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Properties
            .Include(property => property.UnitDefinition)
                .ThenInclude(unit => unit!.Aliases)
            .Include(property => property.Values)
            .FirstOrDefaultAsync(property => property.Id == id, cancellationToken);
    }

    public Task<bool> IsSkuInUseAsync(
        string sku,
        Guid? excludedVariantId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ProductVariants.AnyAsync(
            variant => variant.Sku == sku &&
                       (!excludedVariantId.HasValue || variant.Id != excludedVariantId.Value),
            cancellationToken);
    }
    public void RemoveSpecification(ProductPropertyValue specification)
    {
        dbContext.ProductPropertyValues.Remove(specification);
    }

    public void RemoveVariantSpecification(ProductVariantPropertyValue specification)
    {
        dbContext.ProductVariantPropertyValues.Remove(specification);
    }
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
