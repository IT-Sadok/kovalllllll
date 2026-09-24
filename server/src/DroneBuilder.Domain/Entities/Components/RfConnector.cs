using System.Text.Json.Serialization;

namespace DroneBuilder.Domain.Entities.Components;

[JsonConverter(typeof(JsonStringEnumConverter<RfConnector>))]
public enum RfConnector
{
    Ufl = 0,
    Mmcx = 1,
    Sma = 2,
    RpSma = 3,
    Ipex4 = 4,
    Mcx = 5
}
