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
            .Include(wi => wi.ProductVariant)
                .ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(wi => wi.Id == warehouseItemId, cancellationToken);
    }

    public async Task<WarehouseItem?> GetWarehouseItemByProductIdAsync(Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WarehouseItems
            .Include(wi => wi.ProductVariant)
                .ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(
                wi => wi.ProductVariant!.ProductId == productId && wi.ProductVariant.IsDefault,
                cancellationToken);
    }

    public async Task<PagedResult<WarehouseItem>> GetWarehouseItemsAsync(PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        IOrderedQueryable<WarehouseItem> query = dbContext.WarehouseItems
            .Include(wi => wi.ProductVariant)
                .ThenInclude(v => v!.Product)
            .OrderBy(wi => wi.ProductVariant!.Product!.Name);

        int totalCount = await query.CountAsync(cancellationToken);

        List<WarehouseItem> items = await query
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
            .Include(wi => wi.ProductVariant)
            .Where(wi => productIds.Contains(wi.ProductVariant!.ProductId) && wi.ProductVariant.IsDefault)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
