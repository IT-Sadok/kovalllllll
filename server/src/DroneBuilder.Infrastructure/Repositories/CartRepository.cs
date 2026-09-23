using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure.Repositories;

public class CartRepository(ApplicationDbContext dbContext) : ICartRepository
{
    public async Task CreateCartAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await dbContext.Carts.AddAsync(cart, cancellationToken);
    }

    public async Task<Cart?> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p!.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task RemoveCartItemAsync(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        CartItem? cartItem = await dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId, cancellationToken);

        if (cartItem != null)
        {
            dbContext.CartItems.Remove(cartItem);
        }
    }

    public async Task<Cart?> GetCartByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await dbContext.CartItems
            .FromSql(
                $"""
                 SELECT ci.* FROM "CartItems" ci
                 INNER JOIN "Carts" c ON c."Id" = ci."CartId"
                 WHERE c."UserId" = {userId}
                 FOR UPDATE OF ci
                 """)
            .ToListAsync(cancellationToken);

        return await GetCartByUserIdAsync(userId, cancellationToken);
    }

    public async Task<int> RemoveCartItemsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        List<CartItem> cartItems = await dbContext.CartItems
            .Where(ci => ci.ProductId == productId)
            .ToListAsync(cancellationToken);

        dbContext.CartItems.RemoveRange(cartItems);

        return cartItems.Sum(ci => ci.Quantity);
    }

    public async Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        await dbContext.CartItems
            .Where(ci => ci.CartId == cartId)
            .ForEachAsync(ci => dbContext.CartItems.Remove(ci), cancellationToken);
    }

    public async Task AddCartItemAsync(CartItem cartItem, CancellationToken cancellationToken = default)
    {
        await dbContext.CartItems.AddAsync(cartItem, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
