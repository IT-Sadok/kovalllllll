using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.CartModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.CartQueries;

public class GetCartItemsQueryHandler(ICartRepository cartRepository, IMapper mapper)
    : IQueryHandler<GetCartItemsQuery, ICollection<CartItemModel>>
{
    public async Task<ICollection<CartItemModel>> ExecuteAsync(GetCartItemsQuery itemsQuery,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(itemsQuery.UserId, cancellationToken);

        if (cart == null)
        {
            throw new NotFoundException($"Cart for user with ID {itemsQuery.UserId} not found.");
        }

        return mapper.Map<ICollection<CartItemModel>>(cart.CartItems);
    }
}

public record GetCartItemsQuery(Guid UserId);