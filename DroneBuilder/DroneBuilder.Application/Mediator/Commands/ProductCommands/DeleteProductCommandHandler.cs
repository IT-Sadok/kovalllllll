using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mediator.Commands;

public class DeleteProductCommandHandler(IProductRepository productRepository) : ICommandHandler<DeleteProductCommand>
{
    public async Task ExecuteCommandAsync(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        await productRepository.RemoveProductAsync(command.Model, cancellationToken);
    }
}

public record DeleteProductCommand(Product Model);