namespace DroneBuilder.Application.Models.CartModels;

public class CartModel
{
    public Guid UserId { get; set; }
    public ICollection<CartItemModel> CartItems { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}