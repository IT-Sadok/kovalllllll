using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class PropertyRepository(ApplicationDbContext dbContext) : IPropertyRepository
{
    public async Task AddPropertyAsync(Property property, CancellationToken cancellationToken = default)
    {
        await dbContext.Properties.AddAsync(property, cancellationToken);
    }

    public async Task GetPropertyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var property = await dbContext.Properties.FindAsync([id],
            cancellationToken: cancellationToken);
        if (property == null)
        {
            throw new NotFoundException($"Property with id {id} not found.");
        }
    }

    public async Task<Property> GetPropertyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Properties.FindAsync([id],
            cancellationToken: cancellationToken) ?? throw new NotFoundException($"Property with id {id} not found.");
    }

    public async Task<IEnumerable<Property>> GetPropertiesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Properties.ToListAsync(cancellationToken);
    }

    public async Task RemovePropertyAsync(Property property, CancellationToken cancellationToken = default)
    {
        var existingProperty = await GetPropertyAsync(property.Id, cancellationToken);
        dbContext.Properties.Remove(existingProperty);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePropertyAsync(Property property, CancellationToken cancellationToken = default)
    {
        var existingProperty = await GetPropertyAsync(property.Id, cancellationToken);

        existingProperty.Name = property.Name;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task GetPropertiesByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var existingProduct = await dbContext.Products
            .Include(p => p.Properties)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (existingProduct == null)
        {
            throw new NotFoundException($"Product with id {productId} not found.");
        }
    }


    public async Task GetPropertyValuesAsync(Property property, CancellationToken cancellationToken = default)
    {
        var existingProperty = await GetPropertyAsync(property.Id, cancellationToken);
        await dbContext.Entry(existingProperty)
            .Collection(p => p.Values)
            .LoadAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}