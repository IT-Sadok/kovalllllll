using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.CartModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.CartQueries;

public class GetCartQueryHandler(ICartRepository cartRepository, IMapper mapper, IUserContext userContext)
    : IQueryHandler<GetCartByUserIdQuery, CartModel>
{
    public async Task<CartModel> ExecuteAsync(GetCartByUserIdQuery query, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            throw new NotFoundException($"Cart for user with ID {userContext.UserId} not found.");
        }

        return mapper.Map<CartModel>(cart);
    }
}

public record GetCartByUserIdQuery();