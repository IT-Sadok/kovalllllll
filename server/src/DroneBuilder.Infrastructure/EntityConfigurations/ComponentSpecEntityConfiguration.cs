using DroneBuilder.Domain.Entities.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DroneBuilder.Infrastructure.EntityConfigurations;

public class ComponentSpecEntityConfiguration : IEntityTypeConfiguration<ComponentSpec>
{
    public void Configure(EntityTypeBuilder<ComponentSpec> builder)
    {
        builder.ToTable("ComponentSpecs");

        builder.HasKey(s => s.ProductId);

        builder.HasOne(s => s.Product)
            .WithOne(p => p.Spec)
            .HasForeignKey<ComponentSpec>(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasDiscriminator(s => s.Type)
            .HasValue<FrameSpec>(ComponentType.Frame)
            .HasValue<MotorSpec>(ComponentType.Motor)
            .HasValue<PropellerSpec>(ComponentType.Propeller)
            .HasValue<FlightControllerSpec>(ComponentType.FlightController)
            .HasValue<EscSpec>(ComponentType.Esc)
            .HasValue<BatterySpec>(ComponentType.Battery)
            .HasValue<VideoTransmitterSpec>(ComponentType.VideoTransmitter)
            .HasValue<CameraSpec>(ComponentType.Camera)
            .HasValue<ReceiverSpec>(ComponentType.Receiver)
            .HasValue<AntennaSpec>(ComponentType.Antenna)
            .HasValue<StackSpec>(ComponentType.Stack)
            .HasValue<RadioSpec>(ComponentType.Radio);

        builder.HasIndex(s => s.Type);
    }
}
