using DroneBuilder.Application.Models.CartModels;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Mappings;

public static class CartMappingExtensions
{
    public static CartModel ToModel(this Cart cart)
    {
        if (cart == null)
        {
            return null!;
        }

        return new CartModel
        {
            UserId = cart.UserId,
            CartItems = cart.CartItems?.Select(i => i.ToModel()).ToList() ?? new List<CartItemModel>(),
            TotalPrice = cart.CartItems?.Sum(x => x.Quantity * (x.Product?.Price ?? 0m)) ?? 0m,
            CreatedAt = cart.CreatedAt
        };
    }

    public static Cart ToEntity(this CreateCartModel model)
    {
        if (model == null)
        {
            return null!;
        }

        return new Cart
        {
            UserId = model.UserId,
            CartItems = model.CartItems?.Select(i => i.ToEntity()).ToList() ?? new List<CartItem>(),
            CreatedAt = model.CreatedAt
        };
    }

    public static CartItemModel ToModel(this CartItem item)
    {
        if (item == null)
        {
            return null!;
        }

        return new CartItemModel
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.Product?.Price ?? 0m,
            ProductName = item.Product?.Name ?? item.ProductName,
            ProductImageUrl = (item.Product?.Images != null && item.Product.Images.Any())
                ? (item.Product.Images.FirstOrDefault(x => x.IsPrimary) ?? item.Product.Images.First()).Url
                : string.Empty
        };
    }

    public static CartItem ToEntity(this CreateCartItemModel model)
    {
        if (model == null)
        {
            return null!;
        }

        return new CartItem
        {
            ProductId = model.ProductId,
            Quantity = model.Quantity
        };
    }

    public static CartItem ToEntity(this CartItemModel model)
    {
        if (model == null)
        {
            return null!;
        }

        return new CartItem
        {
            ProductId = model.ProductId,
            Quantity = model.Quantity
        };
    }

    public static void UpdateEntity(this UpdateCartItemModel model, CartItem entity)
    {
        if (model == null || entity == null)
        {
            return;
        }

        entity.Quantity = model.Quantity;
    }
}
