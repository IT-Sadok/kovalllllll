namespace DroneBuilder.Application.Models.CartModels;

public class CreateCartItemModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}