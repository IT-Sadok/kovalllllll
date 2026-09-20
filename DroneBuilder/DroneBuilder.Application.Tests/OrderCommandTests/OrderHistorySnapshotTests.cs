using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Tests.OrderCommandTests;

/// <summary>
/// OrderItem.ProductName is a snapshot taken at purchase time. Renaming a product later must not
/// rewrite what past orders say was bought.
/// </summary>
public class OrderHistorySnapshotTests
{
    [Fact]
    public void ToModel_WhenProductWasRenamed_ShouldKeepTheNameCapturedAtPurchase()
    {
        // Arrange
        var item = new OrderItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Carbon Frame X1",
            Quantity = 1,
            PriceAtPurchase = 120m,
            Product = new Product { Name = "Carbon Frame X2 (renamed)" }
        };

        // Act
        OrderItemModel model = item.ToModel();

        // Assert
        Assert.Equal("Carbon Frame X1", model.ProductName);
    }

    [Fact]
    public void ToModel_WhenSnapshotIsMissing_ShouldFallBackToTheLiveProduct()
    {
        // Arrange -- rows written before the name was captured have an empty snapshot.
        var item = new OrderItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = string.Empty,
            Quantity = 1,
            PriceAtPurchase = 120m,
            Product = new Product { Name = "Carbon Frame X1" }
        };

        // Act
        OrderItemModel model = item.ToModel();

        // Assert
        Assert.Equal("Carbon Frame X1", model.ProductName);
    }

    [Fact]
    public void ToModel_WhenSnapshotIsMissingAndProductIsGone_ShouldReturnEmpty()
    {
        // Arrange
        var item = new OrderItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = string.Empty,
            Quantity = 1,
            PriceAtPurchase = 120m
        };

        // Act
        OrderItemModel model = item.ToModel();

        // Assert
        Assert.Equal(string.Empty, model.ProductName);
    }
}
