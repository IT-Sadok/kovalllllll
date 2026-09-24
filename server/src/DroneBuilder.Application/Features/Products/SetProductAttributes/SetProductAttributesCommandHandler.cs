using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Products.SetProductAttributes;

public class SetProductAttributesCommandHandler(IProductRepository productRepository)
    : ICommandHandler<SetProductAttributesCommand, ProductModel>
{
    public async Task<Result<ProductModel>> ExecuteCommandAsync(SetProductAttributesCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Fail<ProductModel>(new NotFoundError($"Product with id {command.ProductId} not found."));
        }

        product.Attributes.Clear();

        for (int i = 0; i < command.Attributes.Count; i++)
        {
            product.Attributes.Add(new ProductAttribute
            {
                ProductId = product.Id,
                Name = command.Attributes[i].Name.Trim(),
                Value = command.Attributes[i].Value.Trim(),
                SortOrder = i
            });
        }

        await productRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(product.ToModel());
    }
}

public record SetProductAttributesCommand(Guid ProductId, List<ProductAttributeModel> Attributes);
