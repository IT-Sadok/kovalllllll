using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class ValueRepository(ApplicationDbContext dbContext) : IValueRepository
{
    public async Task AddValueAsync(Value value, CancellationToken cancellationToken = default)
    {
        await dbContext.Values.AddAsync(value, cancellationToken);
    }

    public async Task GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existingValue = await dbContext.Values.FindAsync([id], cancellationToken: cancellationToken);
        if (existingValue == null)
        {
            throw new NotFoundException($"Value with {id} found");
        }
    }

    public async Task<Value> GetValueAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Values.FindAsync([id], cancellationToken: cancellationToken) ??
               throw new NotFoundException($"Value with {id} found");
    }

    public async Task<IEnumerable<Value>> GetValuesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Values.ToListAsync(cancellationToken);
    }

    public async Task RemoveValueAsync(Value value, CancellationToken cancellationToken = default)
    {
        var existingValue = await GetValueAsync(value.Id, cancellationToken);
        dbContext.Values.Remove(existingValue);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateValueAsync(Value value, CancellationToken cancellationToken = default)
    {
        var existingValue = await GetValueAsync(value.Id, cancellationToken);

        existingValue.ValueText = value.ValueText;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Value>> GetValuesByPropertyIdAsync(Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        var existingProperty = await dbContext.Properties
            .Include(p => p.Values)
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);

        return existingProperty == null
            ? throw new NotFoundException($"Property with id {propertyId} not found.")
            : existingProperty.Values;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}