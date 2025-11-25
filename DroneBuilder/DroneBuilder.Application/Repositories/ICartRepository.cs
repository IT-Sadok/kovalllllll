using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface ICartRepository
{
    Task CreateCartAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<Cart?> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RemoveCartItemAsync(Guid cartItemId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task AddCartItemAsync(CartItem cartItem, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}