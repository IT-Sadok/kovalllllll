using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class WarehouseMappingExtensions
{
    public static WarehouseItem ToEntity(this CreateWarehouseItemModel model)
    {
        if (model == null) return null!;
        return new WarehouseItem
        {
            WarehouseId = model.WarehouseId,
            ProductId = model.ProductId,
            Quantity = model.Quantity
        };
    }

    public static WarehouseItemModel ToModel(this WarehouseItem item)
    {
        if (item == null) return null!;
        return new WarehouseItemModel
        {
            Id = item.Id,
            WarehouseId = item.WarehouseId,
            ProductId = item.ProductId,
            ProductName = item.Product != null ? item.Product.Name : "Unknown",
            Quantity = item.Quantity
        };
    }

    public static WarehouseModel ToModel(this Warehouse warehouse)
    {
        if (warehouse == null) return null!;
        return new WarehouseModel
        {
            Name = warehouse.Name,
            CreatedAt = warehouse.CreatedAt
        };
    }
}
