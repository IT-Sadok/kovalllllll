using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DroneBuilder.Domain.Entities;

public class Order : AuditableEntity
{
    private static readonly JsonSerializerOptions ShippingJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private string? _legacyShippingDetails;

    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Status Status { get; set; } = Status.New;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public ShippingAddress ShippingAddress { get; set; } = new();

    [NotMapped]
    public string ShippingDetails
    {
        get => _legacyShippingDetails ?? JsonSerializer.Serialize(ShippingAddress, ShippingJsonOptions);
        set
        {
            _legacyShippingDetails = value;
            try
            {
                ShippingAddress = JsonSerializer.Deserialize<ShippingAddress>(value, ShippingJsonOptions) ?? new();
                _legacyShippingDetails = null;
            }
            catch (JsonException)
            {
                ShippingAddress = new ShippingAddress();
            }
        }
    }
}
