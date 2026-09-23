using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

public static class OrderRestockExtensions
{
    public static async Task<Result> RestockAsync(
        this IWarehouseRepository warehouseRepository,
        Order order,
        CancellationToken cancellationToken)
    {
        foreach (OrderItem item in order.OrderItems)
        {
            WarehouseItem? warehouseItem =
                await warehouseRepository.GetWarehouseItemByProductIdAsync(item.ProductId, cancellationToken);

            if (warehouseItem is null)
            {
                Warehouse? warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken);
                if (warehouse is null)
                {
                    return Result.Fail(new NotFoundError("Warehouse not found."));
                }

                warehouseItem = new WarehouseItem
                {
                    WarehouseId = warehouse.Id,
                    ProductId = item.ProductId
                };

                await warehouseRepository.AddWarehouseItemAsync(warehouseItem, cancellationToken);
            }

            warehouseItem.Quantity += item.Quantity;
        }

        return Result.Ok();
    }
}
