using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;
using FluentResults;

namespace DroneBuilder.Application.Features.Products.SetProductSpec;

public class SetProductSpecCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<SetProductSpecCommand, ProductModel>
{
    public async Task<Result<ProductModel>> ExecuteCommandAsync(SetProductSpecCommand command,
        CancellationToken cancellationToken)
    {
        await using ITransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        Product? product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductModel>(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        ComponentSpec spec = command.Spec.ToEntity(product.Id);
        if (product.Category.ToComponentType() != spec.Type)
        {
            return Result.Fail<ProductModel>(new BadRequestError(
                $"A {spec.Type} spec cannot be set on a product in the {product.Category} category."));
        }

        if (product.Spec is not null)
        {
            product.Spec = null;
            await productRepository.SaveChangesAsync(cancellationToken);
        }

        product.Spec = spec;
        product.NeedsReview = false;

        await productRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Ok(product.ToModel());
    }
}

public record SetProductSpecCommand(Guid ProductId, ComponentSpecModel Spec);
