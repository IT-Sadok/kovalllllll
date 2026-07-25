using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Application.Validation;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class UpdateCartItemQuantityCommandHandler(
    ICartRepository cartRepository,
    IWarehouseRepository warehouseRepository,
    IUserContext userContext)
    : ICommandHandler<UpdateCartItemQuantityCommand>
{
    public async Task<Result> ExecuteCommandAsync(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
    {
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);
        if (cart == null)
        {
            return Result.Fail(new NotFoundError($"Cart for user with ID {userContext.UserId} not found."));
        }

        CartItem? cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == command.ProductId);
        if (cartItem == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found in cart."));
        }

        WarehouseItem? warehouseItem = await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);
        if (warehouseItem == null)
        {
            return Result.Fail(new NotFoundError($"Warehouse item for product ID {command.ProductId} not found."));
        }

        DateTime now = DateTime.UtcNow;
        InventoryReservation? reservation = cartItem.Reservation;
        if (reservation is not null && reservation.ExpiresAt <= now)
        {
            warehouseItem.ExpireReservation(reservation, now);
            reservation = null;
        }

        if (command.Quantity > 0)
        {
            Result validationResult = WarehouseValidation.ValidateState(warehouseItem);
            if (validationResult.IsFailed)
            {
                return validationResult;
            }

            int additionalQuantity = reservation is null
                ? command.Quantity
                : Math.Max(0, command.Quantity - reservation.Quantity);

            if (warehouseItem.AvailableQuantity < additionalQuantity)
            {
                return Result.Fail(new BadRequestError("Not enough stock in warehouse."));
            }
        }

        if (command.Quantity == 0)
        {
            if (reservation is not null)
            {
                warehouseItem.ReleaseReservation(reservation, now);
            }

            await cartRepository.RemoveCartItemAsync(cartItem.Id, cancellationToken);
        }
        else if (reservation is null)
        {
            reservation = warehouseItem.CreateReservation(
                cart.Id,
                cartItem.Id,
                command.Quantity,
                InventoryReservationPolicy.NewExpiration(now));
            reservation.Cart = cart;
            reservation.CartItem = cartItem;
            cartItem.Reservation = reservation;
            cartItem.Quantity = command.Quantity;
        }
        else
        {
            warehouseItem.ChangeReservation(
                reservation,
                command.Quantity,
                InventoryReservationPolicy.NewExpiration(now));
            cartItem.Quantity = command.Quantity;
        }

        Result postValidation = WarehouseValidation.ValidateState(warehouseItem);
        if (postValidation.IsFailed)
        {
            return postValidation;
        }

        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record UpdateCartItemQuantityCommand(Guid ProductId, int Quantity);
