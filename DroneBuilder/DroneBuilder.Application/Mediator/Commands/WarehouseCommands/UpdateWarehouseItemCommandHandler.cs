using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.WarehouseCommands;

public class UpdateWarehouseItemCommandHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
    : ICommandHandler<UpdateWarehouseItemCommand, WarehouseItemModel>
{
    public async Task<WarehouseItemModel> ExecuteCommandAsync(UpdateWarehouseItemCommand command,
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

        if (command.Model.Quantity.HasValue)
            warehouseItem.Quantity = command.Model.Quantity.Value;


        await warehouseRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<WarehouseItemModel>(warehouseItem);
    }
}

public record UpdateWarehouseItemCommand(Guid WarehouseItemId, UpdateWarehouseItemModel Model);