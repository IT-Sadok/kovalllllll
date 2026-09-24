using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Products.UpdateProduct;

public class UpdateProductCommandHandler(IProductRepository productRepository)
    : ICommandHandler<UpdateProductCommand, ProductModel>
{
    public async Task<Result<ProductModel>> ExecuteCommandAsync(UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        Product? existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct is null)
        {
            return Result.Fail<ProductModel>(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        if (command.Model.Category.HasValue && existingProduct.Spec is not null
            && command.Model.Category.Value.ToComponentType() != existingProduct.Spec.Type)
        {
            return Result.Fail<ProductModel>(new BadRequestError(
                $"Remove the {existingProduct.Spec.Type} spec before changing the category to {command.Model.Category.Value}."));
        }

        command.Model.UpdateEntity(existingProduct);

        await productRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(existingProduct.ToModel());
    }
}

public record UpdateProductCommand(Guid ProductId, UpdateProductRequestModel Model);
