using System.Text.Json.Serialization;

namespace DroneBuilder.Domain.Entities.Components;

[JsonConverter(typeof(JsonStringEnumConverter<VideoSystem>))]
public enum VideoSystem
{
    Analog = 0,
    DjiO3 = 1,
    DjiO4 = 2,
    Walksnail = 3,
    HdZero = 4
}
