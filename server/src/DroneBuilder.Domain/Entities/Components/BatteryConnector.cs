using System.Text.Json.Serialization;

namespace DroneBuilder.Domain.Entities.Components;

[JsonConverter(typeof(JsonStringEnumConverter<BatteryConnector>))]
public enum BatteryConnector
{
    Xt30 = 0,
    Xt60 = 1,
    Xt90 = 2
}
