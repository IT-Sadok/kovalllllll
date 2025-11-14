using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class UpdateProductCommandHandler(IProductRepository productRepository)
    : ICommandHandler<UpdateProductCommand, ProductResponseModel>
{
    public async Task<ProductResponseModel> ExecuteCommandAsync(UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct is null)
        {
            throw new KeyNotFoundException($"Product with id {command.ProductId} not found.");
        }

        existingProduct.Name = command.RequestModel.Name ?? existingProduct.Name;
        existingProduct.Price = command.RequestModel.Price ?? existingProduct.Price;

        await productRepository.SaveChangesAsync(cancellationToken);

        return new ProductResponseModel
        {
            Id = existingProduct.Id,
            Name = existingProduct.Name,
            Price = existingProduct.Price
        };
    }
}

public record UpdateProductCommand(Guid ProductId, UpdateProductRequestModel RequestModel);