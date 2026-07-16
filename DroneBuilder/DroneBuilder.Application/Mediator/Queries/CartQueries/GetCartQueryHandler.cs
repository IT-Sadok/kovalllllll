using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.CartModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using DroneBuilder.Application.Mappings;
namespace DroneBuilder.Application.Mediator.Queries.CartQueries;

public class GetCartQueryHandler(ICartRepository cartRepository, IUserContext userContext)
    : IQueryHandler<GetCartByUserIdQuery, CartModel>
{
    public async Task<Result<CartModel>> ExecuteAsync(GetCartByUserIdQuery query, CancellationToken cancellationToken)
    {
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);

        if (cart == null)
        {
            return Result.Fail<CartModel>(new NotFoundError($"Cart for user with ID {userContext.UserId} not found."));
        }

        return Result.Ok(cart.ToModel());
    }
}

public record GetCartByUserIdQuery();
