namespace DroneBuilder.Domain.Entities.Components;

public class RadioSpec : ComponentSpec
{
    public RadioSpec() => Type = ComponentType.Radio;

    public RadioProtocol Protocol { get; set; }
}
