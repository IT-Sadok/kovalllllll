using System.Text.Json.Serialization;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Products;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(FrameSpecModel), nameof(ComponentType.Frame))]
[JsonDerivedType(typeof(MotorSpecModel), nameof(ComponentType.Motor))]
[JsonDerivedType(typeof(PropellerSpecModel), nameof(ComponentType.Propeller))]
[JsonDerivedType(typeof(FlightControllerSpecModel), nameof(ComponentType.FlightController))]
[JsonDerivedType(typeof(EscSpecModel), nameof(ComponentType.Esc))]
[JsonDerivedType(typeof(BatterySpecModel), nameof(ComponentType.Battery))]
[JsonDerivedType(typeof(VideoTransmitterSpecModel), nameof(ComponentType.VideoTransmitter))]
[JsonDerivedType(typeof(CameraSpecModel), nameof(ComponentType.Camera))]
[JsonDerivedType(typeof(ReceiverSpecModel), nameof(ComponentType.Receiver))]
[JsonDerivedType(typeof(AntennaSpecModel), nameof(ComponentType.Antenna))]
[JsonDerivedType(typeof(StackSpecModel), nameof(ComponentType.Stack))]
public abstract record ComponentSpecModel;

public record FrameSpecModel(
    decimal MaxPropSizeInch,
    List<MountPattern> FcMountPatterns,
    List<MountPattern> MotorMountPatterns,
    int? CameraWidthMm) : ComponentSpecModel;

public record MotorSpecModel(
    string StatorSize,
    int Kv,
    MountPattern MountPattern,
    int MinCells,
    int MaxCells,
    decimal? MaxCurrentA,
    decimal? ShaftMm,
    int? MaxThrustGrams = null) : ComponentSpecModel;

public record PropellerSpecModel(
    decimal DiameterInch,
    decimal? PitchInch,
    int? BladeCount,
    decimal? HubMm) : ComponentSpecModel;

public record FlightControllerSpecModel(
    MountPattern MountPattern,
    int MinCells,
    int MaxCells) : ComponentSpecModel;

public record EscSpecModel(
    MountPattern MountPattern,
    int MinCells,
    int MaxCells,
    decimal? ContinuousCurrentA,
    BatteryConnector? BatteryConnector) : ComponentSpecModel;

public record BatterySpecModel(
    int Cells,
    int CapacityMah,
    int? CRating,
    BatteryConnector Connector) : ComponentSpecModel;

public record VideoTransmitterSpecModel(
    VideoSystem VideoSystem,
    RfConnector? AntennaConnector,
    MountPattern? MountPattern) : ComponentSpecModel;

public record CameraSpecModel(
    VideoSystem VideoSystem,
    int? WidthMm) : ComponentSpecModel;

public record ReceiverSpecModel(
    RadioProtocol Protocol) : ComponentSpecModel;

public record AntennaSpecModel(
    RfConnector Connector) : ComponentSpecModel;

public record StackSpecModel(
    MountPattern MountPattern,
    int MinCells,
    int MaxCells,
    decimal? ContinuousCurrentA,
    BatteryConnector? BatteryConnector) : ComponentSpecModel;
