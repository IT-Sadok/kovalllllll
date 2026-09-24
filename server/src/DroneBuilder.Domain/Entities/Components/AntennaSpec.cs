namespace DroneBuilder.Domain.Entities.Components;

public class AntennaSpec : ComponentSpec
{
    public AntennaSpec() => Type = ComponentType.Antenna;

    public RfConnector Connector { get; set; }
}
