using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class DeleteProductCommandHandler(IProductRepository productRepository) : ICommandHandler<DeleteProductCommand>
{
    public async Task ExecuteCommandAsync(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct is null)
        {
            throw new NotFoundException($"Product with id {command.ProductId} not found.");
        }

        productRepository.RemoveProduct(existingProduct);
        await productRepository.SaveChangesAsync(cancellationToken);
    }
}

public record DeleteProductCommand(Guid ProductId);