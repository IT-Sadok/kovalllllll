using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Products;

public static class ComponentSpecMappingExtensions
{
    public static ComponentSpecModel ToModel(this ComponentSpec spec) => spec switch
    {
        FrameSpec s => new FrameSpecModel(s.MaxPropSizeInch, s.FcMountPatterns, s.MotorMountPatterns, s.CameraWidthMm),
        MotorSpec s => new MotorSpecModel(s.StatorSize, s.Kv, s.MountPattern, s.MinCells, s.MaxCells, s.MaxCurrentA,
            s.ShaftMm, s.MaxThrustGrams),
        PropellerSpec s => new PropellerSpecModel(s.DiameterInch, s.PitchInch, s.BladeCount, s.HubMm),
        FlightControllerSpec s => new FlightControllerSpecModel(s.MountPattern, s.MinCells, s.MaxCells),
        EscSpec s => new EscSpecModel(s.MountPattern, s.MinCells, s.MaxCells, s.ContinuousCurrentA, s.BatteryConnector),
        BatterySpec s => new BatterySpecModel(s.Cells, s.CapacityMah, s.CRating, s.Connector),
        VideoTransmitterSpec s => new VideoTransmitterSpecModel(s.VideoSystem, s.AntennaConnector, s.MountPattern),
        CameraSpec s => new CameraSpecModel(s.VideoSystem, s.WidthMm),
        ReceiverSpec s => new ReceiverSpecModel(s.Protocol),
        RadioSpec s => new RadioSpecModel(s.Protocol),
        AntennaSpec s => new AntennaSpecModel(s.Connector),
        StackSpec s => new StackSpecModel(s.MountPattern, s.MinCells, s.MaxCells, s.ContinuousCurrentA,
            s.BatteryConnector),
        _ => throw new ArgumentOutOfRangeException(nameof(spec), spec.GetType().Name, null)
    };

    public static ComponentSpec ToEntity(this ComponentSpecModel model, Guid productId)
    {
        ComponentSpec spec = model switch
        {
            FrameSpecModel m => new FrameSpec
            {
                MaxPropSizeInch = m.MaxPropSizeInch,
                FcMountPatterns = m.FcMountPatterns,
                MotorMountPatterns = m.MotorMountPatterns,
                CameraWidthMm = m.CameraWidthMm
            },
            MotorSpecModel m => new MotorSpec
            {
                StatorSize = m.StatorSize,
                Kv = m.Kv,
                MountPattern = m.MountPattern,
                MinCells = m.MinCells,
                MaxCells = m.MaxCells,
                MaxCurrentA = m.MaxCurrentA,
                ShaftMm = m.ShaftMm,
                MaxThrustGrams = m.MaxThrustGrams
            },
            PropellerSpecModel m => new PropellerSpec
            {
                DiameterInch = m.DiameterInch,
                PitchInch = m.PitchInch,
                BladeCount = m.BladeCount,
                HubMm = m.HubMm
            },
            FlightControllerSpecModel m => new FlightControllerSpec
            {
                MountPattern = m.MountPattern,
                MinCells = m.MinCells,
                MaxCells = m.MaxCells
            },
            EscSpecModel m => new EscSpec
            {
                MountPattern = m.MountPattern,
                MinCells = m.MinCells,
                MaxCells = m.MaxCells,
                ContinuousCurrentA = m.ContinuousCurrentA,
                BatteryConnector = m.BatteryConnector
            },
            BatterySpecModel m => new BatterySpec
            {
                Cells = m.Cells,
                CapacityMah = m.CapacityMah,
                CRating = m.CRating,
                Connector = m.Connector
            },
            VideoTransmitterSpecModel m => new VideoTransmitterSpec
            {
                VideoSystem = m.VideoSystem,
                AntennaConnector = m.AntennaConnector,
                MountPattern = m.MountPattern
            },
            CameraSpecModel m => new CameraSpec
            {
                VideoSystem = m.VideoSystem,
                WidthMm = m.WidthMm
            },
            ReceiverSpecModel m => new ReceiverSpec { Protocol = m.Protocol },
            RadioSpecModel m => new RadioSpec { Protocol = m.Protocol },
            AntennaSpecModel m => new AntennaSpec { Connector = m.Connector },
            StackSpecModel m => new StackSpec
            {
                MountPattern = m.MountPattern,
                MinCells = m.MinCells,
                MaxCells = m.MaxCells,
                ContinuousCurrentA = m.ContinuousCurrentA,
                BatteryConnector = m.BatteryConnector
            },
            _ => throw new ArgumentOutOfRangeException(nameof(model), model.GetType().Name, null)
        };

        spec.ProductId = productId;
        return spec;
    }
}
