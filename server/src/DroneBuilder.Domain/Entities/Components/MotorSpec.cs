namespace DroneBuilder.Domain.Entities.Components;

public class MotorSpec : ComponentSpec
{
    public MotorSpec() => Type = ComponentType.Motor;

    public string StatorSize { get; set; } = string.Empty;
    public int Kv { get; set; }
    public MountPattern MountPattern { get; set; }
    public int MinCells { get; set; }
    public int MaxCells { get; set; }
    public decimal? MaxCurrentA { get; set; }
    public decimal? ShaftMm { get; set; }
    public int? MaxThrustGrams { get; set; }
}
