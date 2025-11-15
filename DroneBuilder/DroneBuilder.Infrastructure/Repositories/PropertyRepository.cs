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
        return await dbContext.Properties
            .Include(p => p.Values)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<ICollection<Property>> GetPropertiesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Properties
            .AsNoTracking()
            .Include(p => p.Values)
            .ToListAsync(cancellationToken);
    }

    public Task<Property> GetValuesByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return dbContext.Properties
            .Include(p => p.Values)
            .FirstAsync(p => p.Id == propertyId, cancellationToken);
    }


    public void RemoveProperty(Property property)
    {
        dbContext.Properties.Remove(property);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}