using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class WarehouseMappingExtensions
{
    public static WarehouseItemModel ToModel(this WarehouseItem item)
    {
        return new WarehouseItemModel
        {
            Id = item.Id,
            WarehouseId = item.WarehouseId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? "Unknown",
            Quantity = item.AvailableQuantity
        };
    }

    public static WarehouseModel ToModel(this Warehouse warehouse)
    {
        return new WarehouseModel
        {
            Name = warehouse.Name,
            CreatedAt = warehouse.CreatedAt
        };
    }
}
