using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface ICartRepository
{
    Task CreateCartAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<Cart?> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same as GetCartByUserIdAsync, but locks the cart items for the rest of the transaction so the
    /// reservation sweep cannot restock them halfway through a checkout.
    /// </summary>
    Task<Cart?> GetCartByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RemoveCartItemAsync(Guid cartItemId, CancellationToken cancellationToken = default);
    Task RemoveCartItemsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task AddCartItemAsync(CartItem cartItem, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
