using DroneBuilder.Application.Models.CartModels;
using DroneBuilder.Domain.Entities;
using Mapster;

namespace DroneBuilder.Application.Mappings;

public class CartMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Cart, CartModel>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CartItems, src => src.CartItems)
            .Map(dest => dest.TotalPrice, src => src.CartItems.Sum(x => x.Quantity * x.Product!.Price))
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<CreateCartModel, Cart>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.CartItems, src => src.CartItems)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Ignore(dest => dest.Id);

        config.NewConfig<CartItem, CartItemModel>()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.Price, src => src.Product != null ? src.Product.Price : 0m)
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : src.ProductName);

        config.NewConfig<CreateCartItemModel, CartItem>()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CartId)
            .Ignore(dest => dest.Cart)
            .Ignore(dest => dest.Product);

        config.NewConfig<UpdateCartItemModel, CartItem>()
            .IgnoreNullValues(true)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CartId)
            .Ignore(dest => dest.Cart)
            .Ignore(dest => dest.Product);

        config.NewConfig<Cart, ICollection<CartItemModel>>()
            .Map(dest => dest, src => src.CartItems);
    }
}