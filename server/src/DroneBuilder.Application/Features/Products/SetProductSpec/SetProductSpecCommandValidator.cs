using FluentValidation;

namespace DroneBuilder.Application.Features.Products.SetProductSpec;

public class SetProductSpecCommandValidator : AbstractValidator<SetProductSpecCommand>
{
    public SetProductSpecCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.Spec)
            .NotNull().WithMessage("Component spec is required.")
            .SetInheritanceValidator(v => v
                .Add(new FrameSpecModelValidator())
                .Add(new MotorSpecModelValidator())
                .Add(new PropellerSpecModelValidator())
                .Add(new FlightControllerSpecModelValidator())
                .Add(new EscSpecModelValidator())
                .Add(new BatterySpecModelValidator())
                .Add(new VideoTransmitterSpecModelValidator())
                .Add(new CameraSpecModelValidator())
                .Add(new ReceiverSpecModelValidator())
                .Add(new AntennaSpecModelValidator())
                .Add(new StackSpecModelValidator())
                .Add(new RadioSpecModelValidator()));
    }
}
