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

    public Task<Value?> GetValueByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return ValueQuery().FirstOrDefaultAsync(value => value.Id == id, cancellationToken);
    }

    public Task<Value?> GetValueWithPropertiesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return ValueQuery().FirstOrDefaultAsync(value => value.Id == id, cancellationToken);
    }

    public async Task<ICollection<Value>> GetValuesAsync(CancellationToken cancellationToken = default)
    {
        return await ValueQuery()
            .AsNoTracking()
            .OrderBy(value => value.Text)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsCodeInUseAsync(
        string code,
        Guid? excludedId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Values.AnyAsync(
            value => value.Code == code && (!excludedId.HasValue || value.Id != excludedId.Value),
            cancellationToken);
    }

    public void RemoveValue(Value value)
    {
        dbContext.Values.Remove(value);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Value> ValueQuery()
    {
        return dbContext.Values
            .AsSplitQuery()
            .Include(value => value.Aliases)
            .Include(value => value.Properties)
            .Include(value => value.ProductPropertyValues)
            .Include(value => value.ProductVariantPropertyValues);
    }
}
