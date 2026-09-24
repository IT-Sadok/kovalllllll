namespace DroneBuilder.Domain.Entities.Components;

public class VideoTransmitterSpec : ComponentSpec
{
    public VideoTransmitterSpec() => Type = ComponentType.VideoTransmitter;

    public VideoSystem VideoSystem { get; set; }
    public RfConnector? AntennaConnector { get; set; }
    public MountPattern? MountPattern { get; set; }
}
