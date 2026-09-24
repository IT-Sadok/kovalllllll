namespace DroneBuilder.Domain.Entities.Components;

public class StackSpec : ComponentSpec
{
    public StackSpec() => Type = ComponentType.Stack;

    public MountPattern MountPattern { get; set; }
    public int MinCells { get; set; }
    public int MaxCells { get; set; }
    public decimal? ContinuousCurrentA { get; set; }
    public BatteryConnector? BatteryConnector { get; set; }
}
