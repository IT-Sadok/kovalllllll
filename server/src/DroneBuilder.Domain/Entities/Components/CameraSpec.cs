namespace DroneBuilder.Domain.Entities.Components;

public class CameraSpec : ComponentSpec
{
    public CameraSpec() => Type = ComponentType.Camera;

    public VideoSystem VideoSystem { get; set; }
    public int? WidthMm { get; set; }
}
