using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Carts.GetCart;

public class GetCartQueryHandler(ICartRepository cartRepository, IUserContext userContext)
    : IQueryHandler<GetCartByUserIdQuery, CartModel>
{
    public async Task<Result<CartModel>> ExecuteAsync(GetCartByUserIdQuery query, CancellationToken cancellationToken)
    {
        Cart cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken)
                    ?? new Cart { UserId = userContext.UserId };

        return Result.Ok(cart.ToModel());
    }
}

public record GetCartByUserIdQuery();
