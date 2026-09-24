namespace DroneBuilder.Domain.Entities.Components;

public class ReceiverSpec : ComponentSpec
{
    public ReceiverSpec() => Type = ComponentType.Receiver;

    public RadioProtocol Protocol { get; set; }
}
