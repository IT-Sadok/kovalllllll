using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
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
        await productRepository.SaveChangesAsync(cancellationToken);

        var createdProduct = await productRepository.GetProductByIdAsync(product.Id, cancellationToken);

        return mapper.Map<ProductModel>(createdProduct!);
    }
}

public record CreateProductCommand(CreateProductModel Model);