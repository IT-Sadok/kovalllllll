using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public sealed class CatalogMetadataRepository(ApplicationDbContext dbContext) : ICatalogMetadataRepository
{
    public async Task<ICollection<ComponentType>> GetComponentTypesAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ComponentTypes
            .AsNoTracking()
            .OrderBy(type => type.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<ComponentType?> GetComponentTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.ComponentTypes
            .AsSplitQuery()
            .Include(type => type.Properties)
                .ThenInclude(rule => rule.Property)
                    .ThenInclude(property => property!.UnitDefinition)
                        .ThenInclude(unit => unit!.Aliases)
            .Include(type => type.Properties)
                .ThenInclude(rule => rule.Property)
                    .ThenInclude(property => property!.Values)
            .Include(type => type.Products)
                .ThenInclude(product => product.ProductPropertyValues)
            .Include(type => type.Products)
                .ThenInclude(product => product.Variants)
                    .ThenInclude(variant => variant.Specifications)
            .FirstOrDefaultAsync(type => type.Id == id, cancellationToken);
    }

    public Task<Property?> GetPropertyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Properties
            .Include(property => property.UnitDefinition)
                .ThenInclude(unit => unit!.Aliases)
            .Include(property => property.Values)
            .FirstOrDefaultAsync(property => property.Id == id, cancellationToken);
    }

    public Task<bool> IsComponentTypeCodeInUseAsync(
        string code,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ComponentTypes.AnyAsync(
            type => type.Code == code && (!excludedId.HasValue || type.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task AddComponentTypeAsync(ComponentType componentType, CancellationToken cancellationToken = default)
    {
        await dbContext.ComponentTypes.AddAsync(componentType, cancellationToken);
    }

    public void RemoveComponentTypeRule(ComponentTypeProperty rule)
    {
        dbContext.ComponentTypeProperties.Remove(rule);
    }

    public Task<UnitDefinition?> GetUnitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.UnitDefinitions
            .Include(unit => unit.Aliases)
            .Include(unit => unit.Properties)
            .FirstOrDefaultAsync(unit => unit.Id == id, cancellationToken);
    }

    public Task<bool> IsUnitCodeInUseAsync(
        string code,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.UnitDefinitions.AnyAsync(
            unit => unit.Code == code && (!excludedId.HasValue || unit.Id != excludedId.Value),
            cancellationToken);
    }

    public async Task AddUnitAsync(UnitDefinition unit, CancellationToken cancellationToken = default)
    {
        await dbContext.UnitDefinitions.AddAsync(unit, cancellationToken);
    }

    public void RemoveUnit(UnitDefinition unit)
    {
        dbContext.UnitDefinitions.Remove(unit);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
