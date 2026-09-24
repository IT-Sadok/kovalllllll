namespace DroneBuilder.Domain.Entities.Components;

public class EscSpec : ComponentSpec
{
    public EscSpec() => Type = ComponentType.Esc;

    public MountPattern MountPattern { get; set; }
    public int MinCells { get; set; }
    public int MaxCells { get; set; }
    public decimal? ContinuousCurrentA { get; set; }
    public BatteryConnector? BatteryConnector { get; set; }
}
