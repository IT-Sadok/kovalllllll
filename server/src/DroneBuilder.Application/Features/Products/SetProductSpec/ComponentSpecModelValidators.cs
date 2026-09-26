using FluentValidation;

namespace DroneBuilder.Application.Features.Products.SetProductSpec;

public class FrameSpecModelValidator : AbstractValidator<FrameSpecModel>
{
    public FrameSpecModelValidator()
    {
        RuleFor(x => x.MaxPropSizeInch).InclusiveBetween(1, 13);
        RuleFor(x => x.FcMountPatterns).NotEmpty();
        RuleForEach(x => x.FcMountPatterns).IsInEnum();
        RuleForEach(x => x.MotorMountPatterns).IsInEnum();
        RuleFor(x => x.CameraWidthMm).InclusiveBetween(10, 30);
    }
}

public class MotorSpecModelValidator : AbstractValidator<MotorSpecModel>
{
    public MotorSpecModelValidator()
    {
        RuleFor(x => x.StatorSize)
            .NotEmpty()
            .Matches(@"^\d{4}(\.\d)?$").WithMessage("Stator size must be 4 digits, e.g. 2207 or 2207.5.");
        RuleFor(x => x.Kv).InclusiveBetween(100, 60000);
        RuleFor(x => x.MountPattern).IsInEnum();
        RuleFor(x => x.MinCells).InclusiveBetween(1, 12);
        RuleFor(x => x.MaxCells).InclusiveBetween(1, 12).GreaterThanOrEqualTo(x => x.MinCells);
        RuleFor(x => x.MaxCurrentA).GreaterThan(0);
        RuleFor(x => x.ShaftMm).GreaterThan(0);
        RuleFor(x => x.MaxThrustGrams).InclusiveBetween(1, 50000).When(x => x.MaxThrustGrams.HasValue);
    }
}

public class PropellerSpecModelValidator : AbstractValidator<PropellerSpecModel>
{
    public PropellerSpecModelValidator()
    {
        RuleFor(x => x.DiameterInch).InclusiveBetween(1, 13);
        RuleFor(x => x.PitchInch).GreaterThan(0);
        RuleFor(x => x.BladeCount).InclusiveBetween(2, 6);
        RuleFor(x => x.HubMm).GreaterThan(0);
    }
}

public class FlightControllerSpecModelValidator : AbstractValidator<FlightControllerSpecModel>
{
    public FlightControllerSpecModelValidator()
    {
        RuleFor(x => x.MountPattern).IsInEnum();
        RuleFor(x => x.MinCells).InclusiveBetween(1, 12);
        RuleFor(x => x.MaxCells).InclusiveBetween(1, 12).GreaterThanOrEqualTo(x => x.MinCells);
    }
}

public class EscSpecModelValidator : AbstractValidator<EscSpecModel>
{
    public EscSpecModelValidator()
    {
        RuleFor(x => x.MountPattern).IsInEnum();
        RuleFor(x => x.MinCells).InclusiveBetween(1, 12);
        RuleFor(x => x.MaxCells).InclusiveBetween(1, 12).GreaterThanOrEqualTo(x => x.MinCells);
        RuleFor(x => x.ContinuousCurrentA).GreaterThan(0);
        RuleFor(x => x.BatteryConnector).IsInEnum();
    }
}

public class BatterySpecModelValidator : AbstractValidator<BatterySpecModel>
{
    public BatterySpecModelValidator()
    {
        RuleFor(x => x.Cells).InclusiveBetween(1, 12);
        RuleFor(x => x.CapacityMah).GreaterThan(0);
        RuleFor(x => x.CRating).GreaterThan(0);
        RuleFor(x => x.Connector).IsInEnum();
    }
}

public class VideoTransmitterSpecModelValidator : AbstractValidator<VideoTransmitterSpecModel>
{
    public VideoTransmitterSpecModelValidator()
    {
        RuleFor(x => x.VideoSystem).IsInEnum();
        RuleFor(x => x.AntennaConnector).IsInEnum();
        RuleFor(x => x.MountPattern).IsInEnum();
    }
}

public class CameraSpecModelValidator : AbstractValidator<CameraSpecModel>
{
    public CameraSpecModelValidator()
    {
        RuleFor(x => x.VideoSystem).IsInEnum();
        RuleFor(x => x.WidthMm).InclusiveBetween(10, 30);
    }
}

public class ReceiverSpecModelValidator : AbstractValidator<ReceiverSpecModel>
{
    public ReceiverSpecModelValidator()
    {
        RuleFor(x => x.Protocol).IsInEnum();
    }
}

public class RadioSpecModelValidator : AbstractValidator<RadioSpecModel>
{
    public RadioSpecModelValidator()
    {
        RuleFor(x => x.Protocol).IsInEnum();
    }
}

public class AntennaSpecModelValidator : AbstractValidator<AntennaSpecModel>
{
    public AntennaSpecModelValidator()
    {
        RuleFor(x => x.Connector).IsInEnum();
    }
}

public class StackSpecModelValidator : AbstractValidator<StackSpecModel>
{
    public StackSpecModelValidator()
    {
        RuleFor(x => x.MountPattern).IsInEnum();
        RuleFor(x => x.MinCells).InclusiveBetween(1, 12);
        RuleFor(x => x.MaxCells).InclusiveBetween(1, 12).GreaterThanOrEqualTo(x => x.MinCells);
        RuleFor(x => x.ContinuousCurrentA).GreaterThan(0);
        RuleFor(x => x.BatteryConnector).IsInEnum();
    }
}
