using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ProductEvents;
using FluentResults;
using DroneBuilder.Application.Mappings;
namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig)
    : ICommandHandler<CreateProductCommand, ProductModel>
{
    public async Task<Result<ProductModel>> ExecuteCommandAsync(CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        Warehouse? warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken);
        if (warehouse == null)
        {
            return Result.Fail<ProductModel>(new NotFoundError("Warehouse not found."));
        }

        Product product = command.Model.ToEntity();

        await productRepository.AddProductAsync(product, cancellationToken);

        var warehouseItem = new WarehouseItem
        {
            WarehouseId = warehouse.Id,
            ProductId = product.Id
        };

        await warehouseRepository.AddWarehouseItemAsync(warehouseItem, cancellationToken);

        var @event = new ProductCreatedEvent(product.Id);
        await outboxService.StoreEventAsync(@event, queuesConfig.ProductQueue.Name, cancellationToken);

        await productRepository.SaveChangesAsync(cancellationToken);

        Product? createdProduct = await productRepository.GetProductByIdAsync(product.Id, cancellationToken);

        return Result.Ok(createdProduct!.ToModel());
    }
}

public record CreateProductCommand(CreateProductModel Model);
