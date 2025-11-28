using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.WarehouseCommands;

public class AddQuantityToWarehouseItemCommandHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
    : ICommandHandler<AddQuantityToWarehouseItemCommand, WarehouseItemModel>
{
    public async Task<WarehouseItemModel> ExecuteCommandAsync(AddQuantityToWarehouseItemCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Model.QuantityToAdd <= 0)
            throw new BadRequestException("Quantity to add must be greater than 0.");
        
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

        warehouseItem.Quantity += command.Model.QuantityToAdd;

        await warehouseRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<WarehouseItemModel>(warehouseItem);
    }
}

public record AddQuantityToWarehouseItemCommand(Guid WarehouseItemId, AddQuantityModel Model);