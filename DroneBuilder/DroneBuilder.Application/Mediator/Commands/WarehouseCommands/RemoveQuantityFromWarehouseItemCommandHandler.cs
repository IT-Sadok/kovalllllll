using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Application.Validation;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.WarehouseEvents;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Commands.WarehouseCommands;

public class RemoveQuantityFromWarehouseItemCommandHandler(
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<RemoveQuantityFromWarehouseItemCommand, WarehouseItemModel>
{
    public async Task<Result<WarehouseItemModel>> ExecuteCommandAsync(RemoveQuantityFromWarehouseItemCommand command,
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

        Result validationResult = WarehouseValidation.ValidateState(warehouseItem);
        if (validationResult.IsFailed)
        {
            return validationResult.ToResult<WarehouseItemModel>();
        }

        warehouseItem.Quantity -= command.Model.QuantityToRemove;

        validationResult = WarehouseValidation.ValidateState(warehouseItem);
        if (validationResult.IsFailed)
        {
            return validationResult.ToResult<WarehouseItemModel>();
        }

        var @event = new RemovedQuantityFromWarehouseItemEvent(warehouseItem.Id, command.Model.QuantityToRemove);
        await outboxService.StoreEventAsync(@event, queuesConfig.WarehouseQueue.Name, cancellationToken);

        await warehouseRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(warehouseItem.ToModel());
    }
}

public record RemoveQuantityFromWarehouseItemCommand(Guid WarehouseItemId, RemoveQuantityModel Model);
