using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.Validation;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.CartEvents;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class AddItemToCartCommandHandler(
    ICartRepository cartRepository,
    IOutboxEventService outboxService,
    IWarehouseRepository warehouseRepository,
    IProductRepository productRepository,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext) : ICommandHandler<AddItemToCartCommand>
{
    public async Task ExecuteCommandAsync(AddItemToCartCommand command,
        CancellationToken cancellationToken)
    {
        var existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct == null)
        {
            throw new NotFoundException($"Product with ID {command.ProductId} not found.");
        }

        var warehouseItem =
            await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);
        if (warehouseItem == null)
        {
            throw new NotFoundException($"Warehouse item for product ID {command.ProductId} not found.");
        }

        if (command.Quantity <= 0)
        {
            throw new BadRequestException("Quantity must be greater than zero.");
        }

        WarehouseValidation.ValidateState(warehouseItem);


        var cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userContext.UserId,
                CartItems = new List<CartItem>()
            };
            await cartRepository.CreateCartAsync(cart, cancellationToken);
        }

        var existingCartItem = cart.CartItems
            .FirstOrDefault(ci => ci.ProductId == command.ProductId);


        if (existingCartItem == null)
        {
            var newCartItem = new CartItem
            {
                ProductId = command.ProductId,
                ProductName = existingProduct.Name,
                Quantity = command.Quantity,
                Cart = cart
            };
            await cartRepository.AddCartItemAsync(newCartItem, cancellationToken);
        }
        else
        {
            existingCartItem.Quantity += command.Quantity;
        }

        warehouseItem.Quantity -= command.Quantity;

        WarehouseValidation.ValidateState(warehouseItem);

        var @event = new AddedItemToCartEvent(userContext.UserId, command.ProductId, existingProduct.Name,
            command.Quantity);
        await outboxService.PublishEventAsync(@event, queuesConfig.CartQueue, cancellationToken);

        await cartRepository.SaveChangesAsync(cancellationToken);
    }
}

public record AddItemToCartCommand(Guid ProductId, int Quantity);