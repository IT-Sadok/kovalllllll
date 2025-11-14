using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class CreateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
    : ICommandHandler<CreateProductCommand, ProductResponseModel>
{
    public async Task<ProductResponseModel> ExecuteCommandAsync(CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = mapper.Map<Product>(command.RequestModel);

        await productRepository.AddProductAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ProductResponseModel>(product);
    }
}

public record CreateProductCommand(CreateProductRequestModel RequestModel);