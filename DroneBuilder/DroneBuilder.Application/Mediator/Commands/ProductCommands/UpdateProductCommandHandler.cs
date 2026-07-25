using DroneBuilder.Application.Common;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

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

        if (command.Model.Name is not null)
        {
            existingProduct.Name = command.Model.Name;
        }

        if (command.Model.Price.HasValue)
        {
            existingProduct.EnsureDefaultVariant();
            existingProduct.Price = command.Model.Price.Value;
        }

        if (command.Model.Category is not null)
        {
            ProductCategory? category = await productRepository.GetCategoryByNameAsync(
                command.Model.Category,
                cancellationToken);
            if (category is null)
            {
                category = new ProductCategory
                {
                    Name = command.Model.Category.Trim(),
                    Code = EntityCode.FromName(command.Model.Category)
                };
                await productRepository.AddCategoryAsync(category, cancellationToken);
            }

            existingProduct.AssignCategory(category);
        }

        await productRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(existingProduct.ToModel());
    }
}

public record UpdateProductCommand(Guid ProductId, UpdateProductRequestModel Model);
