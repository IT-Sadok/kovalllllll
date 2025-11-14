using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class ValueRepository(ApplicationDbContext dbContext) : IValueRepository
{
    public async Task AddValueAsync(Value value, CancellationToken cancellationToken = default)
    {
        await dbContext.Values.AddAsync(value, cancellationToken);
    }

    public async Task<Value?> GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Values.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<ICollection<Value>> GetValuesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Values.ToListAsync(cancellationToken);
    }

    public async Task<ICollection<Value>> GetValuesByPropertyIdAsync(Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Properties
            .Where(p => p.Id == propertyId)
            .SelectMany(p => p.Values!)
            .ToListAsync(cancellationToken);
    }

    public void RemoveValue(Value value)
    {
        dbContext.Values.Remove(value);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}