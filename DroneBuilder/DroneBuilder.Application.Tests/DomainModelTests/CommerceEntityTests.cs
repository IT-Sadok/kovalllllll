using System.Text.Json;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Tests.DomainModelTests;

public class CommerceEntityTests
{
    [Fact]
    public void EnsureDefaultVariant_CreatesSingleSkuAndKeepsLegacyPriceContract()
    {
        var product = new Product { Price = 149.99m };

        ProductVariant first = product.EnsureDefaultVariant();
        ProductVariant second = product.EnsureDefaultVariant();

        Assert.Same(first, second);
        Assert.True(first.IsDefault);
        Assert.Equal(product.Id, first.ProductId);
        Assert.Equal(149.99m, first.Price);
        Assert.Equal(149.99m, product.Price);
    }

    [Fact]
    public void AssignCategory_UsesNormalizedCategoryEntityForLegacyApiValue()
    {
        var product = new Product { Category = "Temporary" };
        var category = new ProductCategory { Code = "motors", Name = "Motors" };

        product.AssignCategory(category);

        Assert.Equal(category.Id, product.ProductCategoryId);
        Assert.Same(category, product.ProductCategory);
        Assert.Equal("Motors", product.Category);
    }

    [Fact]
    public void WarehouseReservation_SeparatesAvailableAndPhysicalStock()
    {
        var item = new WarehouseItem { Quantity = 10 };

        item.Reserve(4);
        Assert.Equal(10, item.Quantity);
        Assert.Equal(4, item.ReservedQuantity);
        Assert.Equal(6, item.AvailableQuantity);

        item.Release(1);
        Assert.Equal(3, item.ReservedQuantity);

        item.CommitReservation(3);
        Assert.Equal(7, item.Quantity);
        Assert.Equal(0, item.ReservedQuantity);
        Assert.Equal(7, item.AvailableQuantity);
    }

    [Fact]
    public void WarehouseReservation_RejectsQuantityAboveAvailability()
    {
        var item = new WarehouseItem { Quantity = 2 };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => item.Reserve(3));

        Assert.Equal("Not enough available stock.", exception.Message);
    }

    [Fact]
    public void ShippingDetails_LegacyJsonContractRoundTripsStructuredAddress()
    {
        var order = new Order
        {
            ShippingAddress = new ShippingAddress
            {
                FullName = "Test User",
                AddressLine1 = "Main Street 1",
                City = "Kyiv",
                PostalCode = "01001",
                Country = "Ukraine",
                PhoneNumber = "+380000000000"
            }
        };

        using JsonDocument document = JsonDocument.Parse(order.ShippingDetails);

        Assert.Equal("Test User", document.RootElement.GetProperty("fullName").GetString());
        Assert.Equal("Kyiv", document.RootElement.GetProperty("city").GetString());
    }
}
