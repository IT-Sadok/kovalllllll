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
        return await dbContext.Properties.FindAsync([id],
            cancellationToken: cancellationToken);
    }

    public async Task<ICollection<Property>> GetPropertiesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Properties.ToListAsync(cancellationToken);
    }

    public async Task<ICollection<Property>> GetPropertiesByValueIdAsync(Guid valueId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Values
            .Where(v => v.Id == valueId)
            .SelectMany(v => v.Properties!)
            .ToListAsync(cancellationToken);
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