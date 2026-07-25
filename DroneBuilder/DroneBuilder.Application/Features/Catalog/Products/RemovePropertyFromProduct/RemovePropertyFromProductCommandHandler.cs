using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Products.RemovePropertyFromProduct;

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
                try
                {
                    foreach (ProductPropertyValue item in itemsToRemove)
                    {
                        product.RemoveSpecification(item);
                    }
                }
                catch (InvalidOperationException exception)
                {
                    return Result.Fail(new ConflictError(exception.Message));
                }

                await productRepository.SaveChangesAsync(cancellationToken);
            }
        }

        return Result.Ok();
    }
}

public record RemovePropertyFromProductCommand(Guid ProductId, Guid PropertyId);
