using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Validation;

public class WarehouseValidation
{
    public static Result ValidateState(WarehouseItem? warehouseItem)
    {
        if (warehouseItem == null)
        {
            return Result.Fail(new NotFoundError("Warehouse item not found."));
        }

        if (warehouseItem.Quantity < 0 || warehouseItem.ReservedQuantity < 0)
        {
            return Result.Fail(new BadRequestError("Total quantity cannot be negative."));
        }

        if (warehouseItem.ReservedQuantity > warehouseItem.Quantity)
        {
            return Result.Fail(new BadRequestError("Reserved quantity cannot exceed total quantity."));
        }

        return Result.Ok();
    }

    public static Result EnsureEnoughAvailable(WarehouseItem warehouseItem, int requested)
    {
        if (requested <= 0)
        {
            return Result.Fail(new BadRequestError("Quantity must be greater than zero."));
        }

        if (warehouseItem.AvailableQuantity < requested)
        {
            return Result.Fail(new BadRequestError(
                $"Not enough stock. Available: {warehouseItem.AvailableQuantity}, requested: {requested}."));
        }

        return Result.Ok();
    }
}
