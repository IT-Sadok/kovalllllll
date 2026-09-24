namespace DroneBuilder.Domain.Entities.Components;

public class FrameSpec : ComponentSpec
{
    public FrameSpec() => Type = ComponentType.Frame;

    public decimal MaxPropSizeInch { get; set; }
    public List<MountPattern> FcMountPatterns { get; set; } = [];
    public List<MountPattern> MotorMountPatterns { get; set; } = [];
    public int? CameraWidthMm { get; set; }
}
