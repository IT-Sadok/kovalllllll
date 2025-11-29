using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetWarehouseAsync(CancellationToken cancellationToken = default);
    Task AddWarehouseItemAsync(WarehouseItem warehouseItem, CancellationToken cancellationToken = default);
    Task<WarehouseItem?> GetWarehouseItemByIdAsync(Guid warehouseItemId, CancellationToken cancellationToken = default);

    Task<WarehouseItem?>
        GetWarehouseItemByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<ICollection<WarehouseItem>> GetAllWarehouseItemsByProductIdsAsync(ICollection<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}