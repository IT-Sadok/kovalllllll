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
    }

    public static void EnsureEnoughAvailable(WarehouseItem warehouseItem, int requested)
    {
        if (requested <= 0)
            throw new BadRequestException("Quantity must be greater than zero.");
    }
}