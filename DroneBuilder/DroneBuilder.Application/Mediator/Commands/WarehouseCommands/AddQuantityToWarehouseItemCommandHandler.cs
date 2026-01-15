using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Events.WarehouseEvents;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.WarehouseCommands;

public class AddQuantityToWarehouseItemCommandHandler(
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IMapper mapper)
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

        var warehouseItem =
            await warehouseRepository.GetWarehouseItemByIdAsync(command.WarehouseItemId, cancellationToken);

        if (warehouseItem == null)
        {
            throw new NotFoundException($"Warehouse item with id {command.WarehouseItemId} not found.");
        }

        warehouseItem.Quantity += command.Model.QuantityToAdd;

        var @event = new AddedQuantityToWarehouseItemEvent(warehouseItem.Id, command.Model.QuantityToAdd);
        await outboxService.StoreEventAsync(@event, queuesConfig.WarehouseQueue.Name, cancellationToken);

        await warehouseRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<WarehouseItemModel>(warehouseItem);
    }
}

public record AddQuantityToWarehouseItemCommand(Guid WarehouseItemId, AddQuantityModel Model);