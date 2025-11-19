using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class UpdateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
    : ICommandHandler<UpdateProductCommand, ProductModel>
{
    public async Task<ProductModel> ExecuteCommandAsync(UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct is null)
        {
            throw new NotFoundException($"Product with id {command.ProductId} not found.");
        }

        if (command.Model.Name is not null)
            existingProduct.Name = command.Model.Name;

        if (command.Model.Price.HasValue)
            existingProduct.Price = command.Model.Price.Value;

        if (command.Model.Category is not null)
            existingProduct.Category = command.Model.Category;

        await productRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ProductModel>(existingProduct);
    }
}

public record UpdateProductCommand(Guid ProductId, UpdateProductRequestModel Model);