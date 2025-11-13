using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Repositories;

namespace DroneBuilder.Application.Mediator.Commands;

public class UpdateProductCommandHandler(IProductRepository productRepository)
    : ICommandHandler<UpdateProductCommand, ProductResponseModel>
{
    public async Task<ProductResponseModel> ExecuteCommandAsync(UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetProductAsync(command.ProductId, cancellationToken);

        product.Name = command.RequestModel.Name ?? product.Name;
        product.Price = command.RequestModel.Price ?? product.Price;

        await productRepository.UpdateProductAsync(product, cancellationToken);

        return new ProductResponseModel()
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}

public record UpdateProductCommand(Guid ProductId, UpdateProductRequestModel RequestModel);