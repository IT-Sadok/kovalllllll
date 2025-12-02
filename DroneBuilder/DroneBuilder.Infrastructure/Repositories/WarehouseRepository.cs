using DroneBuilder.Application.Models;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class WarehouseRepository(ApplicationDbContext dbContext) : IWarehouseRepository
{
    public async Task<Warehouse?> GetWarehouseAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Warehouses
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

    public async Task<PagedResult<WarehouseItem>> GetWarehouseItemsAsync(PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.WarehouseItems
            .Include(wi => wi.Product)
            .OrderBy(wi => wi.Product!.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<WarehouseItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
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