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

namespace DroneBuilder.Application.Features.Cart.AddItemToCart;

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

        ProductVariant defaultVariant = existingProduct.EnsureDefaultVariant();

        WarehouseItem? warehouseItem =
            await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);
        if (warehouseItem == null)
        {
            return Result.Fail(new NotFoundError($"Warehouse item for product ID {command.ProductId} not found."));
        }

        Result validationResult = WarehouseValidation.ValidateState(warehouseItem);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        validationResult = WarehouseValidation.EnsureEnoughAvailable(warehouseItem, command.Quantity);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        Domain.Entities.Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new Domain.Entities.Cart
            {
                UserId = userContext.UserId,
                CartItems = new List<CartItem>()
            };
            await cartRepository.CreateCartAsync(cart, cancellationToken);
        }

        CartItem? existingCartItem = cart.CartItems
            .FirstOrDefault(ci => ci.ProductId == command.ProductId);

        DateTime expiresAt = InventoryReservationPolicy.NewExpiration(DateTime.UtcNow);

        if (existingCartItem == null)
        {
            var newCartItem = new CartItem
            {
                ProductName = existingProduct.Name,
                Quantity = command.Quantity,
                Cart = cart
            };
            newCartItem.AttachVariant(defaultVariant);
            InventoryReservation reservation = warehouseItem.CreateReservation(
                cart.Id,
                newCartItem.Id,
                command.Quantity,
                expiresAt);
            reservation.Cart = cart;
            reservation.CartItem = newCartItem;
            newCartItem.Reservation = reservation;
            await cartRepository.AddCartItemAsync(newCartItem, cancellationToken);
        }
        else
        {
            int requestedTotal = existingCartItem.Quantity + command.Quantity;
            InventoryReservation? reservation = existingCartItem.Reservation;
            if (reservation is not null && reservation.ExpiresAt <= DateTime.UtcNow)
            {
                warehouseItem.ExpireReservation(reservation, DateTime.UtcNow);
                reservation = null;
            }

            if (reservation is null)
            {
                Result totalQuantityValidation =
                    WarehouseValidation.EnsureEnoughAvailable(warehouseItem, requestedTotal);
                if (totalQuantityValidation.IsFailed)
                {
                    return totalQuantityValidation;
                }

                reservation = warehouseItem.CreateReservation(
                    cart.Id,
                    existingCartItem.Id,
                    requestedTotal,
                    expiresAt);
                reservation.Cart = cart;
                reservation.CartItem = existingCartItem;
                existingCartItem.Reservation = reservation;
            }
            else
            {
                warehouseItem.ChangeReservation(
                    reservation,
                    requestedTotal,
                    expiresAt);
            }

            existingCartItem.Quantity = requestedTotal;
        }

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
