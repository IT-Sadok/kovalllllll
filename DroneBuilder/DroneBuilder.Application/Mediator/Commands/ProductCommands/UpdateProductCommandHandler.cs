using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class UpdateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
    : ICommandHandler<UpdateProductCommand, ProductResponseModel>
{
    public async Task<ProductResponseModel> ExecuteCommandAsync(UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct is null)
        {
            throw new NotFoundException($"Product with id {command.ProductId} not found.");
        }

        existingProduct.Name = command.RequestModel.Name ?? existingProduct.Name;
        existingProduct.Price = command.RequestModel.Price ?? existingProduct.Price;

        await productRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ProductResponseModel>(existingProduct);
    }
}

public record UpdateProductCommand(Guid ProductId, UpdateProductRequestModel RequestModel);