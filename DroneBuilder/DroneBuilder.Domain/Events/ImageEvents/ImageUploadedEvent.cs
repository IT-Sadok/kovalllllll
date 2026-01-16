namespace DroneBuilder.Domain.Events.ImageEvents;

public class ImageUploadedEvent(Guid imageId, Guid productId) : DomainEvent
{
    public Guid ImageId { get; } = imageId;
    public Guid ProductId { get; } = productId;
}