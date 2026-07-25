using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class PropertyRepository(ApplicationDbContext dbContext) : IPropertyRepository
{
    public async Task AddPropertyAsync(Property property, CancellationToken cancellationToken = default)
    {
        await dbContext.Properties.AddAsync(property, cancellationToken);
    }

    public async Task<Property?> GetPropertyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await PropertyQuery().FirstOrDefaultAsync(property => property.Id == id, cancellationToken);
    }

    public async Task<ICollection<Property>> GetPropertiesAsync(CancellationToken cancellationToken = default)
    {
        return await PropertyQuery()
            .AsNoTracking()
            .OrderBy(property => property.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Property?> GetValuesByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return PropertyQuery().FirstOrDefaultAsync(property => property.Id == propertyId, cancellationToken);
    }

    public Task<UnitDefinition?> GetUnitDefinitionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.UnitDefinitions.FirstOrDefaultAsync(unit => unit.Id == id, cancellationToken);
    }

    public Task<bool> IsCodeInUseAsync(
        string code,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Properties.AnyAsync(
            property => property.Code == code && (!excludedId.HasValue || property.Id != excludedId.Value),
            cancellationToken);
    }

    public void RemoveProperty(Property property)
    {
        dbContext.Properties.Remove(property);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Property> PropertyQuery()
    {
        return dbContext.Properties
            .AsSplitQuery()
            .Include(property => property.UnitDefinition)
            .Include(property => property.Values)
                .ThenInclude(value => value.Aliases)
            .Include(property => property.Aliases)
            .Include(property => property.ComponentTypes)
            .Include(property => property.ProductPropertyValues)
            .Include(property => property.ProductVariantPropertyValues);
    }
}
