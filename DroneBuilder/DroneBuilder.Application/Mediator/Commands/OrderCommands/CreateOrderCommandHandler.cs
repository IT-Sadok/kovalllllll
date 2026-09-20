using System.Text.Json;
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
    IUnitOfWork unitOfWork,
    MessageQueuesConfiguration queuesConfig,
    IUserContext userContext) : ICommandHandler<CreateOrderCommand, OrderModel>
{
    public async Task<Result<OrderModel>> ExecuteCommandAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // Creating the order and emptying the cart have to be one atomic step. Otherwise the
        // reservation sweep can expire these very items in between, restocking the warehouse for an
        // order that was created anyway, and the stock ends up counted twice.
        await using ITransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        Cart? cart = await cartRepository.GetCartByUserIdForUpdateAsync(userContext.UserId, cancellationToken);

        // Also the outcome when the sweep won the race and already released the reservation.
        if (cart is null || cart.CartItems.Count == 0)
        {
            return Result.Fail<OrderModel>(new BadRequestError("Cart is empty."));
        }

        var productIds = cart.CartItems.Select(ci => ci.ProductId).ToList();

        ICollection<WarehouseItem> warehouseItems = await warehouseRepository
            .GetAllWarehouseItemsByProductIdsAsync(productIds, cancellationToken);

        // The repository returns only the products it actually stocks, so each cart item has to be
        // looked up individually. Quantities are not re-checked here: stock was already taken out of
        // the warehouse when the item went into the cart.
        var stockedProductIds = warehouseItems.Select(wi => wi.ProductId).ToHashSet();

        foreach (CartItem item in cart.CartItems)
        {
            if (!stockedProductIds.Contains(item.ProductId))
            {
                return Result.Fail<OrderModel>(new NotFoundError($"Product {item.ProductId} not found in warehouse."));
            }
        }

        ICollection<Product> products = await productRepository.GetProductsByIdsAsync(productIds, cancellationToken);

        var orderItems = cart.CartItems.Select(ci =>
        {
            Product product = products.First(p => p.Id == ci.ProductId);

            return new OrderItem
            {
                ProductId = ci.ProductId,
                // Captured alongside the price so a later rename does not rewrite order history.
                ProductName = product.Name,
                Quantity = ci.Quantity,
                PriceAtPurchase = product.Price
            };
        }).ToList();

        var order = new Order
        {
            UserId = userContext.UserId,
            ShippingDetails = JsonSerializer.Serialize(command.ShippingDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            OrderItems = orderItems,
            TotalPrice = orderItems.Sum(i => i.PriceAtPurchase * i.Quantity)
        };

        await orderRepository.CreateOrderAsync(order, cancellationToken);
        await cartRepository.ClearCartAsync(cart.Id, cancellationToken);

        var @event = new OrderCreatedEvent(order.Id, userContext.UserId);
        await outboxService.StoreEventAsync(@event, queuesConfig.OrderQueue.Name, cancellationToken);

        await orderRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Ok(order.ToModel());
    }
}

public record CreateOrderCommand(ShippingDetailsModel ShippingDetails);
