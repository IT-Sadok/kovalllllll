using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Application.Validation;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.CartEvents;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class AddItemToCartCommandHandler(
    ICartRepository cartRepository,
    IOutboxEventService outboxService,
    IWarehouseRepository warehouseRepository,
    IProductRepository productRepository,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext) : ICommandHandler<AddItemToCartCommand>
{
    public async Task<Result> ExecuteCommandAsync(AddItemToCartCommand command,
        CancellationToken cancellationToken)
    {
        Product? existingProduct = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (existingProduct == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found."));
        }

        WarehouseItem? warehouseItem =
            await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);
        if (warehouseItem == null)
        {
            return Result.Fail(new NotFoundError($"Warehouse item for product ID {command.ProductId} not found."));
        }

        if (command.Quantity <= 0)
        {
            return Result.Fail(new BadRequestError("Quantity must be greater than zero."));
        }

        Result validationResult = WarehouseValidation.ValidateState(warehouseItem);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userContext.UserId,
                CartItems = new List<CartItem>()
            };
            await cartRepository.CreateCartAsync(cart, cancellationToken);
        }

        CartItem? existingCartItem = cart.CartItems
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

        validationResult = WarehouseValidation.ValidateState(warehouseItem);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        var @event = new AddedItemToCartEvent(userContext.UserId, command.ProductId, existingProduct.Name,
            command.Quantity);
        await outboxService.StoreEventAsync(@event, queuesConfig.CartQueue.Name, cancellationToken);

        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record AddItemToCartCommand(Guid ProductId, int Quantity);
