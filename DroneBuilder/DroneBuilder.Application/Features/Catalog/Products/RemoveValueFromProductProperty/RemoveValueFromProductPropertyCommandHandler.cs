using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Products.RemoveValueFromProductProperty;

public class RemoveValueFromProductPropertyCommandHandler(IProductRepository productRepository)
    : ICommandHandler<RemoveValueFromProductPropertyCommand>
{
    public async Task<Result> ExecuteCommandAsync(RemoveValueFromProductPropertyCommand command, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found."));
        }

        if (product.ProductPropertyValues != null)
        {
            ProductPropertyValue? item = product.ProductPropertyValues.FirstOrDefault(p => p.PropertyId == command.PropertyId && p.ValueId == command.ValueId);
            if (item != null)
            {
                try
                {
                    product.RemoveSpecification(item);
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

public record RemoveValueFromProductPropertyCommand(Guid ProductId, Guid PropertyId, Guid ValueId);
