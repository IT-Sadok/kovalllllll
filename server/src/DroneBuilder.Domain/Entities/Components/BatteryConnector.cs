using System.Text.Json.Serialization;

namespace DroneBuilder.Domain.Entities.Components;

[JsonConverter(typeof(JsonStringEnumConverter<BatteryConnector>))]
public enum BatteryConnector
{
    Xt30 = 0,
    Xt60 = 1,
    Xt90 = 2,
    Xt150 = 3,
    Ec5 = 4,
    Bt20 = 5,
    Ph20 = 6
}
