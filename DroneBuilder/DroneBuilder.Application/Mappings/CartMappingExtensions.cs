using DroneBuilder.Application.Models.CartModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class CartMappingExtensions
{
    public static CartModel ToModel(this Cart cart)
    {
        return new CartModel
        {
            UserId = cart.UserId,
            CartItems = cart.CartItems.Select(item => item.ToModel()).ToList(),
            TotalPrice = cart.CartItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0m)),
            CreatedAt = cart.CreatedAt
        };
    }

    public static CartItemModel ToModel(this CartItem item)
    {
        return new CartItemModel
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.Product?.Price ?? 0m,
            ProductName = item.Product?.Name ?? item.ProductName,
            ProductImageUrl = item.Product?.Images.Count > 0
                ? (item.Product.Images.FirstOrDefault(image => image.IsPrimary) ?? item.Product.Images.First()).Url
                : string.Empty
        };
    }
}
