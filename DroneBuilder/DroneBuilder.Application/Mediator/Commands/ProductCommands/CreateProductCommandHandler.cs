using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.ProductEvents;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IMapper mapper)
    : ICommandHandler<CreateProductCommand, ProductModel>
{
    public async Task<ProductModel> ExecuteCommandAsync(CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetWarehouseAsync(cancellationToken);
        if (warehouse == null)
        {
            throw new NotFoundException("Warehouse not found.");
        }

        var product = mapper.Map<Product>(command.Model);

        var property = mapper.Map<Property>(command.Model.Properties);


        product.Properties?.Add(property);

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

        var createdProduct = await productRepository.GetProductByIdAsync(product.Id, cancellationToken);

        return mapper.Map<ProductModel>(createdProduct!);
    }
}

public record CreateProductCommand(CreateProductModel Model);