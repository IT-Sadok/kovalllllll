using System.Text.Json.Serialization;

namespace DroneBuilder.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter<ImportRunStatus>))]
public enum ImportRunStatus
{
    Queued = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3
}
