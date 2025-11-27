using DroneBuilder.Application.Exceptions;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Validation;

public class WarehouseValidation
{
    public static void ValidateState(WarehouseItem warehouseItem)
    {
        if (warehouseItem == null)
            throw new NotFoundException("Warehouse item not found.");

        if (warehouseItem.Quantity < 0)
            throw new InvalidOperationException("Total quantity cannot be negative.");

        if (warehouseItem.ReservedQuantity < 0)
            throw new InvalidOperationException("Reserved quantity cannot be negative.");

        if (warehouseItem.AvailableQuantity < 0)
            throw new InvalidOperationException("Available quantity cannot be negative.");

        if (warehouseItem.Quantity != warehouseItem.AvailableQuantity + warehouseItem.ReservedQuantity)
            throw new InvalidOperationException("Invalid warehouse state: quantities don't match.");
    }

    public static void EnsureEnoughAvailable(WarehouseItem warehouseItem, int requested)
    {
        if (requested <= 0)
            throw new BadRequestException("Quantity must be greater than zero.");

        if (requested > warehouseItem.AvailableQuantity)
            throw new BadRequestException("Cannot reserve more than available.");
    }
}