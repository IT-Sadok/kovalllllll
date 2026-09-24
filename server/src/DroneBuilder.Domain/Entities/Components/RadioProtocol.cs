using System.Text.Json.Serialization;

namespace DroneBuilder.Domain.Entities.Components;

[JsonConverter(typeof(JsonStringEnumConverter<RadioProtocol>))]
public enum RadioProtocol
{
    ExpressLrs = 0,
    Crossfire = 1,
    Tracer = 2,
    Ghost = 3,
    FrSky = 4
}
