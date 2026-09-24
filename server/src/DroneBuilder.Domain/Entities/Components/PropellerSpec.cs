namespace DroneBuilder.Domain.Entities.Components;

public class PropellerSpec : ComponentSpec
{
    public PropellerSpec() => Type = ComponentType.Propeller;

    public decimal DiameterInch { get; set; }
    public decimal PitchInch { get; set; }
    public int BladeCount { get; set; }
    public decimal HubMm { get; set; }
}
