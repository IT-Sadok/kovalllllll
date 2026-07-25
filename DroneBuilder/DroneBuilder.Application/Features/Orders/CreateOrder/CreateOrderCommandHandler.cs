using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.OrderEvents;
using FluentResults;

namespace DroneBuilder.Application.Features.Orders.CreateOrder;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IWarehouseRepository warehouseRepository,
    IOutboxEventService outboxService,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext) : ICommandHandler<CreateOrderCommand, OrderModel>
{
    public async Task<Result<OrderModel>> ExecuteCommandAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        Domain.Entities.Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);
        if (cart is null || cart.CartItems.Count == 0)
        {
            return Result.Fail<OrderModel>(new BadRequestError("Cart is empty."));
        }

        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();

        ICollection<WarehouseItem>? warehouseItem = await warehouseRepository
            .GetAllWarehouseItemsByProductIdsAsync(productIds, cancellationToken);

        foreach (CartItem item in cart.CartItems)
        {
            WarehouseItem? inventory = warehouseItem?
                .FirstOrDefault(w => w.ProductId == item.ProductId);
            if (inventory is null)
            {
                return Result.Fail<OrderModel>(new NotFoundError($"Product {item.ProductId} not found in warehouse."));
            }

            if (inventory.ReservedQuantity < item.Quantity)
            {
                return Result.Fail<OrderModel>(new BadRequestError(
                    $"Product {item.ProductId} no longer has a valid stock reservation."));
            }

            if (item.Reservation is null)
            {
                return Result.Fail<OrderModel>(new BadRequestError(
                    $"Product {item.ProductId} no longer has a stock reservation."));
            }

            InventoryReservation reservation = item.Reservation;
            if (reservation.Status != InventoryReservationStatus.Active ||
                reservation.CartId != cart.Id ||
                reservation.CartItemId != item.Id ||
                reservation.WarehouseItemId != inventory.Id ||
                reservation.Quantity != item.Quantity)
            {
                return Result.Fail<OrderModel>(new ConflictError(
                    $"Stock reservation for product {item.ProductId} is inconsistent with the cart."));
            }

            DateTime now = DateTime.UtcNow;
            if (reservation.ExpiresAt <= now)
            {
                inventory.ExpireReservation(reservation, now);
                return Result.Fail<OrderModel>(new BadRequestError(
                    $"Stock reservation for product {item.ProductId} has expired."));
            }

            inventory.CommitReservation(reservation, now);
        }

        ICollection<Product> products = await productRepository.GetProductsByIdsAsync(productIds, cancellationToken);
        Dictionary<Guid, Product> productsById = products.ToDictionary(product => product.Id);
        if (productIds.Distinct().Any(productId => !productsById.ContainsKey(productId)))
        {
            return Result.Fail<OrderModel>(new ConflictError(
                "One or more cart products are no longer available."));
        }

        var orderItems = cart.CartItems.Select(ci =>
        {
            Product product = productsById[ci.ProductId];
            ProductVariant variant = product.EnsureDefaultVariant();

            return new OrderItem
            {
                ProductId = ci.ProductId,
                ProductVariantId = variant.Id,
                ProductVariant = variant,
                ProductName = product.Name,
                Sku = variant.Sku,
                Quantity = ci.Quantity,
                PriceAtPurchase = variant.Price,
                CurrencyCode = variant.CurrencyCode
            };
        }).ToList();

        string[] currencies = orderItems.Select(item => item.CurrencyCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (currencies.Length != 1)
        {
            return Result.Fail<OrderModel>(new ConflictError(
                "All order items must use the same currency."));
        }

        var order = new Order
        {
            UserId = userContext.UserId,
            ShippingAddress = new ShippingAddress
            {
                FullName = command.ShippingDetails.FullName,
                AddressLine1 = command.ShippingDetails.AddressLine1,
                AddressLine2 = command.ShippingDetails.AddressLine2,
                City = command.ShippingDetails.City,
                State = command.ShippingDetails.State,
                PostalCode = command.ShippingDetails.PostalCode,
                Country = command.ShippingDetails.Country,
                PhoneNumber = command.ShippingDetails.PhoneNumber
            },
            OrderItems = orderItems,
            TotalPrice = orderItems.Sum(i => i.PriceAtPurchase * i.Quantity),
            CurrencyCode = currencies[0].ToUpperInvariant()
        };

        await orderRepository.CreateOrderAsync(order, cancellationToken);
        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);

        var @event = new OrderCreatedEvent(order.Id, userContext.UserId);
        await outboxService.StoreEventAsync(@event, queuesConfig.OrderQueue.Name, cancellationToken);

        await orderRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok(order.ToModel());
    }
}

public record CreateOrderCommand(ShippingDetailsModel ShippingDetails);
