using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class WarehouseRepository(ApplicationDbContext dbContext) : IWarehouseRepository
{
    public async Task<Warehouse?> GetWarehouseAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Warehouses
            .Include(w => w.WarehouseItems)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddWarehouseItemAsync(WarehouseItem warehouseItem, CancellationToken cancellationToken = default)
    {
        await dbContext.WarehouseItems.AddAsync(warehouseItem, cancellationToken);
    }

    public async Task<WarehouseItem?> GetWarehouseItemByIdAsync(Guid warehouseItemId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WarehouseItems
            .Include(wi => wi.Product)
            .FirstOrDefaultAsync(wi => wi.Id == warehouseItemId, cancellationToken);
    }

    public async Task<WarehouseItem?> GetWarehouseItemByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WarehouseItems
            .Include(wi => wi.Product)
            .FirstOrDefaultAsync(wi => wi.ProductId == productId, cancellationToken);
    }

    public async Task<ICollection<WarehouseItem>> GetAllWarehouseItemsByProductIdsAsync(ICollection<Guid> productIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WarehouseItems
            .AsNoTracking()
            .Where(wi => productIds.Contains(wi.ProductId))
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}