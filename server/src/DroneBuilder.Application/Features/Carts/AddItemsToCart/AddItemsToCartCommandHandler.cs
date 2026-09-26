using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Builds.CheckBuild;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.CartEvents;
using FluentResults;

namespace DroneBuilder.Application.Features.Carts.AddItemsToCart;

public class AddItemsToCartCommandHandler(
    ICartRepository cartRepository,
    IOutboxEventService outboxService,
    IWarehouseRepository warehouseRepository,
    IProductRepository productRepository,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext) : ICommandHandler<AddItemsToCartCommand>
{
    public async Task<Result> ExecuteCommandAsync(AddItemsToCartCommand command, CancellationToken cancellationToken)
    {
        List<Guid> productIds = command.Items.Select(i => i.ProductId).ToList();

        Dictionary<Guid, Product> products =
            (await productRepository.GetProductsByIdsAsync(productIds, cancellationToken)).ToDictionary(p => p.Id);
        List<Guid> unknown = productIds.Where(id => !products.ContainsKey(id)).ToList();
        if (unknown.Count > 0)
        {
            return Result.Fail(new NotFoundError($"Products not found: {string.Join(", ", unknown)}."));
        }

        Dictionary<Guid, WarehouseItem> stock =
            (await warehouseRepository.GetTrackedWarehouseItemsByProductIdsAsync(productIds, cancellationToken))
            .ToDictionary(w => w.ProductId);

        List<string> shortages = command.Items
            .Where(i => !stock.TryGetValue(i.ProductId, out WarehouseItem? item) || item.Quantity < i.Quantity)
            .Select(i => $"{products[i.ProductId].Name} (available {stock.GetValueOrDefault(i.ProductId)?.Quantity ?? 0}, requested {i.Quantity})")
            .ToList();
        if (shortages.Count > 0)
        {
            return Result.Fail(new BadRequestError($"Not enough stock for: {string.Join("; ", shortages)}."));
        }

        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);
        if (cart == null)
        {
            cart = new Cart { UserId = userContext.UserId, CartItems = new List<CartItem>() };
            await cartRepository.CreateCartAsync(cart, cancellationToken);
        }

        foreach (BuildItemModel item in command.Items)
        {
            Product product = products[item.ProductId];
            CartItem? existing = cart.CartItems.FirstOrDefault(ci => ci.ProductId == item.ProductId);

            if (existing == null)
            {
                await cartRepository.AddCartItemAsync(new CartItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    ReservedAt = DateTime.UtcNow,
                    Cart = cart
                }, cancellationToken);
            }
            else
            {
                existing.Quantity += item.Quantity;
                existing.ReservedAt = DateTime.UtcNow;
            }

            stock[item.ProductId].Quantity -= item.Quantity;

            var @event = new AddedItemToCartEvent(userContext.UserId, item.ProductId, product.Name, item.Quantity);
            await outboxService.StoreEventAsync(@event, queuesConfig.CartQueue.Name, cancellationToken);
        }

        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record AddItemsToCartCommand(List<BuildItemModel> Items);
