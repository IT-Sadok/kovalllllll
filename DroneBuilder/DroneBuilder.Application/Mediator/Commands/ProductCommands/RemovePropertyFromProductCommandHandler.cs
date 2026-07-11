using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class RemovePropertyFromProductCommandHandler(IProductRepository productRepository)
    : ICommandHandler<RemovePropertyFromProductCommand>
{
    public async Task<Result> ExecuteCommandAsync(RemovePropertyFromProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found."));
        }

        if (product.ProductPropertyValues != null)
        {
            var itemsToRemove = product.ProductPropertyValues.Where(p => p.PropertyId == command.PropertyId).ToList();
            if (itemsToRemove.Any())
            {
                foreach (ProductPropertyValue? item in itemsToRemove)
                {
                    product.ProductPropertyValues.Remove(item);
                }

                await productRepository.SaveChangesAsync(cancellationToken);
            }
        }

        return Result.Ok();
    }
}

public record RemovePropertyFromProductCommand(Guid ProductId, Guid PropertyId);
