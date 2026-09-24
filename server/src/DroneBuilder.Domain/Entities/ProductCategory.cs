using System.Text.Json.Serialization;

namespace DroneBuilder.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter<ProductCategory>))]
public enum ProductCategory
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
    Stack = 10,
    Goggles = 100,
    Radio = 101,
    Charger = 102,
    Tool = 103,
    ReadyToFly = 104,
    Accessory = 105
}
