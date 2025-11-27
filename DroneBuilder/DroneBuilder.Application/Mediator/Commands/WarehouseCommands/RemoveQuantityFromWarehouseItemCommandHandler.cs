using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.WarehouseCommands;

public class RemoveQuantityFromWarehouseItemCommandHandler(
    IWarehouseRepository warehouseRepository,
    IMapper mapper)
    : ICommandHandler<RemoveQuantityFromWarehouseItemCommand, WarehouseItemModel>
{
    public async Task<WarehouseItemModel> ExecuteCommandAsync(RemoveQuantityFromWarehouseItemCommand command,
        CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken);
        if (warehouse == null)
        {
            throw new NotFoundException("Warehouse not found.");
        }

        var warehouseItem = warehouse.WarehouseItems
            .FirstOrDefault(wi => wi.Id == command.WarehouseItemId);

        if (warehouseItem == null)
        {
            throw new NotFoundException($"Warehouse item with id {command.WarehouseItemId} not found.");
        }

        if (command.Model.QuantityToRemove <= 0)
            throw new BadRequestException("Quantity to remove must be greater than 0.");

        ValidateWarehouseState(warehouseItem);

        if (command.Model.QuantityToRemove > warehouseItem.AvailableQuantity)
            throw new BadRequestException("Cannot remove more than available quantity.");

        warehouseItem.Quantity -= command.Model.QuantityToRemove;
        warehouseItem.AvailableQuantity -= command.Model.QuantityToRemove;

        ValidateWarehouseState(warehouseItem);

        await warehouseRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<WarehouseItemModel>(warehouseItem);
    }

    private static void ValidateWarehouseState(WarehouseItem item)
    {
        if (item.Quantity < 0 ||
            item.ReservedQuantity < 0 ||
            item.AvailableQuantity < 0)
            throw new ValidationException("Warehouse quantities cannot be negative.");

        if (item.AvailableQuantity + item.ReservedQuantity != item.Quantity)
            throw new ValidationException("Invalid warehouse state: Available + Reserved != Total.");

        if (item.ReservedQuantity > item.Quantity)
            throw new ValidationException("Reserved exceeds total quantity.");
    }
}

public record RemoveQuantityFromWarehouseItemCommand(Guid WarehouseItemId, RemoveQuantityModel Model);