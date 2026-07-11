using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.CartModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.CartQueries;

public class GetCartItemsQueryHandler(ICartRepository cartRepository, IMapper mapper, IUserContext userContext)
    : IQueryHandler<GetCartItemsQuery, ICollection<CartItemModel>>
{
    public async Task<Result<ICollection<CartItemModel>>> ExecuteAsync(GetCartItemsQuery itemsQuery,
        CancellationToken cancellationToken)
    {
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            return Result.Fail<ICollection<CartItemModel>>(new NotFoundError($"Cart for user with ID {userContext.UserId} not found."));
        }

        return Result.Ok(mapper.Map<ICollection<CartItemModel>>(cart.CartItems));
    }
}

public record GetCartItemsQuery();
