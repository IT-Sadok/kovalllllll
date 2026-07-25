using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.WarehouseEvents;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Commands.WarehouseCommands;

public class AddQuantityToWarehouseItemCommandHandler(
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<AddQuantityToWarehouseItemCommand, WarehouseItemModel>
{
    public async Task<Result<WarehouseItemModel>> ExecuteCommandAsync(AddQuantityToWarehouseItemCommand command,
        CancellationToken cancellationToken)
    {
        Warehouse? warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken);
        if (warehouse == null)
        {
            return Result.Fail<WarehouseItemModel>(new NotFoundError("Warehouse not found."));
        }

        WarehouseItem? warehouseItem =
            await warehouseRepository.GetWarehouseItemByIdAsync(command.WarehouseItemId, cancellationToken);

        if (warehouseItem == null)
        {
            return Result.Fail<WarehouseItemModel>(new NotFoundError($"Warehouse item with id {command.WarehouseItemId} not found."));
        }

        warehouseItem.Quantity += command.Model.QuantityToAdd;
        warehouseItem.Version++;

        var @event = new AddedQuantityToWarehouseItemEvent(warehouseItem.Id, command.Model.QuantityToAdd);
        await outboxService.StoreEventAsync(@event, queuesConfig.WarehouseQueue.Name, cancellationToken);

        await warehouseRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(warehouseItem.ToModel());
    }
}

public record AddQuantityToWarehouseItemCommand(Guid WarehouseItemId, AddQuantityModel Model);
