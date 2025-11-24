using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models.CartModels;

public class CreateCartModel
{
    public Guid UserId { get; set; }
    public ICollection<CartItemModel> CartItems { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}