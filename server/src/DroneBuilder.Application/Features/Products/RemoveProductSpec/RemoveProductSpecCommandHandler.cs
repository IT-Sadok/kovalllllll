using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Products.RemoveProductSpec;

public class RemoveProductSpecCommandHandler(IProductRepository productRepository)
    : ICommandHandler<RemoveProductSpecCommand>
{
    public async Task<Result> ExecuteCommandAsync(RemoveProductSpecCommand command, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        if (product.Spec is null)
        {
            return Result.Fail(new NotFoundError($"Product with id {command.ProductId} has no component spec."));
        }

        product.Spec = null;

        await productRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record RemoveProductSpecCommand(Guid ProductId);
