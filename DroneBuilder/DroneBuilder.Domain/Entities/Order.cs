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

    public void MarkAsPaid()
    {
        ChangeStatus(Status.Paid);
    }

    public void ChangeStatus(Status newStatus)
    {
        if (!Enum.IsDefined(newStatus))
        {
            throw new ArgumentOutOfRangeException(nameof(newStatus));
        }

        bool transitionAllowed = Status switch
        {
            Status.New => newStatus is Status.Paid or Status.Cancelled,
            Status.Paid => newStatus is Status.Sent or Status.Cancelled,
            Status.Sent => newStatus is Status.Completed,
            Status.Completed or Status.Cancelled => false,
            _ => false
        };

        if (!transitionAllowed)
        {
            throw new InvalidOperationException($"Order status cannot change from {Status} to {newStatus}.");
        }

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}
