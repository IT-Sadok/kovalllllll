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
namespace DroneBuilder.Application.Mediator.Commands.OrderCommands;

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
        Cart? cart = await cartRepository.GetCartByUserIdAsync(userContext.UserId, cancellationToken);
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

            DateTime now = DateTime.UtcNow;
            if (item.Reservation.ExpiresAt <= now)
            {
                return Result.Fail<OrderModel>(new BadRequestError(
                    $"Stock reservation for product {item.ProductId} has expired."));
            }

            inventory.CommitReservation(item.Reservation, now);
        }

        ICollection<Product> products = await productRepository.GetProductsByIdsAsync(productIds, cancellationToken);

        var orderItems = cart.CartItems.Select(ci =>
        {
            Product product = products.First(p => p.Id == ci.ProductId);
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
            TotalPrice = orderItems.Sum(i => i.PriceAtPurchase * i.Quantity)
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
