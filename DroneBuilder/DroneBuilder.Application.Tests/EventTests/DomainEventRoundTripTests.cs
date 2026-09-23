using System.Text.Json;
using DroneBuilder.Domain.Events;
using DroneBuilder.Domain.Events.CartEvents;
using DroneBuilder.Domain.Events.ImageEvents;
using DroneBuilder.Domain.Events.OrderEvents;
using DroneBuilder.Domain.Events.ProductEvents;
using DroneBuilder.Domain.Events.UserEvents;
using DroneBuilder.Domain.Events.WarehouseEvents;

namespace DroneBuilder.Application.Tests.EventTests;

public class DomainEventRoundTripTests
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static T RoundTrip<T>(T @event) where T : DomainEvent
    {
        string json = JsonSerializer.Serialize(@event, WriteOptions);
        return JsonSerializer.Deserialize<T>(json, ReadOptions)!;
    }

    [Fact]
    public void UserSignedUpEvent_ShouldSurviveRoundTrip()
    {
        var userId = Guid.NewGuid();

        UserSignedUpEvent result = RoundTrip(new UserSignedUpEvent(userId, "pilot@example.com"));

        Assert.Equal(userId, result.UserId);
        Assert.Equal("pilot@example.com", result.Email);
    }

    [Fact]
    public void UserSignedInEvent_ShouldSurviveRoundTrip()
    {
        var userId = Guid.NewGuid();

        UserSignedInEvent result = RoundTrip(new UserSignedInEvent(userId, "pilot@example.com"));

        Assert.Equal(userId, result.UserId);
        Assert.Equal("pilot@example.com", result.Email);
    }

    [Fact]
    public void AddedItemToCartEvent_ShouldSurviveRoundTrip()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        AddedItemToCartEvent result = RoundTrip(new AddedItemToCartEvent(userId, productId, "Frame", 4));

        Assert.Equal(userId, result.UserId);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal("Frame", result.ProductName);
        Assert.Equal(4, result.Quantity);
    }

    [Fact]
    public void ClearedCartEvent_ShouldSurviveRoundTrip()
    {
        var userId = Guid.NewGuid();

        ClearedCartEvent result = RoundTrip(new ClearedCartEvent(userId));

        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public void UpdatedCartItemQuantityEvent_ShouldSurviveRoundTrip()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        UpdatedCartItemQuantityEvent result = RoundTrip(new UpdatedCartItemQuantityEvent(userId, productId, 7));

        Assert.Equal(userId, result.UserId);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal(7, result.Quantity);
    }

    [Fact]
    public void OrderCreatedEvent_ShouldSurviveRoundTrip()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        OrderCreatedEvent result = RoundTrip(new OrderCreatedEvent(orderId, userId));

        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public void ProductCreatedEvent_ShouldSurviveRoundTrip()
    {
        var productId = Guid.NewGuid();

        ProductCreatedEvent result = RoundTrip(new ProductCreatedEvent(productId));

        Assert.Equal(productId, result.ProductId);
    }

    [Fact]
    public void ImageUploadedEvent_ShouldSurviveRoundTrip()
    {
        var imageId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        ImageUploadedEvent result = RoundTrip(new ImageUploadedEvent(imageId, productId));

        Assert.Equal(imageId, result.ImageId);
        Assert.Equal(productId, result.ProductId);
    }

    [Fact]
    public void AddedQuantityToWarehouseItemEvent_ShouldSurviveRoundTrip()
    {
        var warehouseItemId = Guid.NewGuid();

        AddedQuantityToWarehouseItemEvent result =
            RoundTrip(new AddedQuantityToWarehouseItemEvent(warehouseItemId, 12));

        Assert.Equal(warehouseItemId, result.WarehouseItemId);
        Assert.Equal(12, result.QuantityAdded);
    }

    [Fact]
    public void RemovedQuantityFromWarehouseItemEvent_ShouldSurviveRoundTrip()
    {
        var warehouseItemId = Guid.NewGuid();

        RemovedQuantityFromWarehouseItemEvent result =
            RoundTrip(new RemovedQuantityFromWarehouseItemEvent(warehouseItemId, 12));

        Assert.Equal(warehouseItemId, result.WarehouseItemId);
        Assert.Equal(12, result.QuantityRemoved);
    }

    [Fact]
    public void RoundTrip_ShouldPreserveTheEventTypeDiscriminator()
    {
        var @event = new OrderCreatedEvent(Guid.NewGuid(), Guid.NewGuid());

        OrderCreatedEvent result = RoundTrip(@event);

        Assert.Equal(typeof(OrderCreatedEvent).FullName, result.Type);
        Assert.Equal(@event.Id, result.Id);
    }
}
