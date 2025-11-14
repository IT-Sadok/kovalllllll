using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class CreateProductCommandHandler(IProductRepository productRepository)
    : ICommandHandler<CreateProductCommand, ProductResponseModel>
{
    public async Task<ProductResponseModel> ExecuteCommandAsync(CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.RequestModel.Name,
            Price = command.RequestModel.Price,
            Images = command.RequestModel.Images,
            Properties = command.RequestModel.Properties
        };

        await productRepository.AddProductAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);

        return new ProductResponseModel()
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}

public record CreateProductCommand(CreateProductRequestModel RequestModel);