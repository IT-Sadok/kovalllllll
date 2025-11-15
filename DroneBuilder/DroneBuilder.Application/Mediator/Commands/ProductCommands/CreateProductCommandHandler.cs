using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class CreateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
    : ICommandHandler<CreateProductCommand, ProductResponseModel>
{
    public async Task<ProductResponseModel> ExecuteCommandAsync(CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = mapper.Map<Product>(command.Model);

        var property = mapper.Map<Property>(command.Model.Properties);

        product.Properties?.Add(property);

        await productRepository.AddProductAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);

        var createdProduct = await productRepository.GetProductByIdAsync(product.Id, cancellationToken);

        return mapper.Map<ProductResponseModel>(createdProduct!);
    }
}

public record CreateProductCommand(CreateProductModel Model);