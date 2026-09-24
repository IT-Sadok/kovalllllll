namespace DroneBuilder.Domain.Entities.Components;

public class BatterySpec : ComponentSpec
{
    public BatterySpec() => Type = ComponentType.Battery;

    public int Cells { get; set; }
    public int CapacityMah { get; set; }
    public int CRating { get; set; }
    public BatteryConnector Connector { get; set; }
}
