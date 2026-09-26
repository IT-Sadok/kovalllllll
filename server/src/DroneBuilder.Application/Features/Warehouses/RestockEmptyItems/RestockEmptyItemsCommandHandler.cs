using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.WarehouseEvents;
using FluentResults;

namespace DroneBuilder.Application.Features.Warehouses.RestockEmptyItems;

public class RestockEmptyItemsCommandHandler(
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<RestockEmptyItemsCommand, RestockResultModel>
{
    public async Task<Result<RestockResultModel>> ExecuteCommandAsync(RestockEmptyItemsCommand command,
        CancellationToken cancellationToken)
    {
        ICollection<WarehouseItem> emptyItems = await warehouseRepository.GetEmptyWarehouseItemsAsync(cancellationToken);

        foreach (WarehouseItem item in emptyItems)
        {
            item.Quantity = command.Quantity;

            var @event = new AddedQuantityToWarehouseItemEvent(item.Id, command.Quantity);
            await outboxService.StoreEventAsync(@event, queuesConfig.WarehouseQueue.Name, cancellationToken);
        }

        await warehouseRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(new RestockResultModel(emptyItems.Count));
    }
}

public record RestockEmptyItemsCommand(int Quantity);

public record RestockResultModel(int RestockedItems);
