using System.Text.Json.Serialization;

namespace DroneBuilder.Domain.Entities.Components;

[JsonConverter(typeof(JsonStringEnumConverter<MountPattern>))]
public enum MountPattern
{
    M9x9 = 0,
    M12x12 = 1,
    M16x16 = 2,
    M19x19 = 3,
    M20x20 = 4,
    M25_5x25_5 = 5,
    M30_5x30_5 = 6
}
