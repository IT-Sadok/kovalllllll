using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.CartModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using FluentResults;

namespace DroneBuilder.Application.Features.Cart.GetCartItems;

public class GetCartItemsQueryHandler(ICartRepository cartRepository, IUserContext userContext)
    : IQueryHandler<GetCartItemsQuery, ICollection<CartItemModel>>
{
    public async Task<Result<ICollection<CartItemModel>>> ExecuteAsync(GetCartItemsQuery itemsQuery,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            return Result.Fail<ICollection<CartItemModel>>(new NotFoundError($"Cart for user with ID {userContext.UserId} not found."));
        }

        return Result.Ok<ICollection<CartItemModel>>(cart.CartItems.Select(x => x.ToModel()).ToList());
    }
}

public record GetCartItemsQuery();
