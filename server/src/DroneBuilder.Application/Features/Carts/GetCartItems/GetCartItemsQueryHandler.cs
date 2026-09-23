using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Features.Carts.GetCartItems;

public class GetCartItemsQueryHandler(ICartRepository cartRepository, IUserContext userContext)
    : IQueryHandler<GetCartItemsQuery, ICollection<CartItemModel>>
{
    public async Task<Result<ICollection<CartItemModel>>> ExecuteAsync(GetCartItemsQuery itemsQuery,
        CancellationToken cancellationToken)
    {
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        ICollection<CartItemModel> items = cart?.CartItems.Select(x => x.ToModel()).ToList() ?? [];

        return Result.Ok(items);
    }
}

public record GetCartItemsQuery();
