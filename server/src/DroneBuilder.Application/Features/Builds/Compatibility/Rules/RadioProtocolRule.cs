using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Builds.Compatibility.Rules;

public class RadioProtocolRule : ICompatibilityRule
{
    public IEnumerable<CompatibilityIssue> Check(BuildParts build)
    {
        foreach (SpecPart<RadioSpec> radio in build.With<RadioSpec>())
        {
            foreach (SpecPart<ReceiverSpec> receiver in build.With<ReceiverSpec>())
            {
                if (radio.Spec.Protocol != receiver.Spec.Protocol)
                {
                    yield return new CompatibilityIssue(IssueSeverity.Error, "radio_protocol",
                        $"{radio.Name} speaks {SpecLabels.Of(radio.Spec.Protocol)}, but {receiver.Name} is {SpecLabels.Of(receiver.Spec.Protocol)}.",
                        [radio.Id, receiver.Id]);
                }
            }
        }
    }
}
