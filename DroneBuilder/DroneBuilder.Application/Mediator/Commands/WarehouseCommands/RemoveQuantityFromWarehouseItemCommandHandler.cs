using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.Validation;
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
        if (command.Model.QuantityToRemove <= 0)
            throw new BadRequestException("Quantity to remove must be greater than 0.");

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

        WarehouseValidation.ValidateState(warehouseItem);

        warehouseItem.Quantity -= command.Model.QuantityToRemove;

        WarehouseValidation.ValidateState(warehouseItem);

        await warehouseRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<WarehouseItemModel>(warehouseItem);
    }
}

public record RemoveQuantityFromWarehouseItemCommand(Guid WarehouseItemId, RemoveQuantityModel Model);