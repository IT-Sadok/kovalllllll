namespace DroneBuilder.Application.Features.Carts.AddItemToCart;

public class CreateCartItemModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
