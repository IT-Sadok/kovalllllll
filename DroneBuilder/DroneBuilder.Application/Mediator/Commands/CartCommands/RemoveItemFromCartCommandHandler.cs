using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Application.Validation;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.CartCommands;

public class RemoveItemFromCartCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IUserContext userContext)
    : ICommandHandler<RemoveItemFromCartCommand>
{
    public async Task<Result> ExecuteCommandAsync(RemoveItemFromCartCommand command, CancellationToken cancellationToken)
    {
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            return Result.Fail(new NotFoundError($"Cart for User ID {userContext.UserId} not found."));
        }

        Product? product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);

        if (product == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found."));
        }

        CartItem? cartItem = cart.CartItems.FirstOrDefault(item => item.ProductId == command.ProductId);

        if (cartItem == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found in the cart."));
        }

        WarehouseItem? warehouseItem =
            await warehouseRepository.GetWarehouseItemByProductIdAsync(command.ProductId, cancellationToken);

        if (warehouseItem == null)
        {
            return Result.Fail(new NotFoundError($"Warehouse item for Product ID {command.ProductId} not found."));
        }

        Result validationResult = WarehouseValidation.ValidateState(warehouseItem);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        if (cartItem.Reservation is not null)
        {
            warehouseItem.ReleaseReservation(cartItem.Reservation, DateTime.UtcNow);
        }

        validationResult = WarehouseValidation.ValidateState(warehouseItem);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        await cartRepository.RemoveCartItemAsync(cartItem.Id, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record RemoveItemFromCartCommand(Guid ProductId);
