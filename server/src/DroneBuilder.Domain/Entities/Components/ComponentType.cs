using System.Text.Json.Serialization;

namespace DroneBuilder.Domain.Entities.Components;

[JsonConverter(typeof(JsonStringEnumConverter<ComponentType>))]
public enum ComponentType
{
    Frame = 0,
    Motor = 1,
    Propeller = 2,
    FlightController = 3,
    Esc = 4,
    Battery = 5,
    VideoTransmitter = 6,
    Camera = 7,
    Receiver = 8,
    Antenna = 9,
    Stack = 10
}
