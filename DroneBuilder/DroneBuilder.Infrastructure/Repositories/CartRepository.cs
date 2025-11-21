using DroneBuilder.Application.Repositories;
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
            .Include(x => x.CartItems)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task RemoveCartItemAsync(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var cartItem = await dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId, cancellationToken);

        if (cartItem != null) dbContext.CartItems.Remove(cartItem);
    }

    public async Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        await dbContext.CartItems
            .Where(ci => ci.CartId == cartId)
            .ForEachAsync(ci => dbContext.CartItems.Remove(ci), cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}