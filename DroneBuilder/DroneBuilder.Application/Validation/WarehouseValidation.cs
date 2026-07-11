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

        if (warehouseItem.Quantity < 0)
        {
            return Result.Fail(new BadRequestError("Total quantity cannot be negative."));
        }

        return Result.Ok();
    }

    public static Result EnsureEnoughAvailable(WarehouseItem warehouseItem, int requested)
    {
        if (requested <= 0)
        {
            return Result.Fail(new BadRequestError("Quantity must be greater than zero."));
        }

        if (warehouseItem.Quantity < requested)
        {
            return Result.Fail(new BadRequestError($"Not enough stock. Available: {warehouseItem.Quantity}, requested: {requested}."));
        }

        return Result.Ok();
    }
}
