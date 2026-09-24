namespace DroneBuilder.Domain.Entities.Components;

public class FlightControllerSpec : ComponentSpec
{
    public FlightControllerSpec() => Type = ComponentType.FlightController;

    public MountPattern MountPattern { get; set; }
    public int MinCells { get; set; }
    public int MaxCells { get; set; }
}
