using System.Text.Json;

namespace DroneBuilder.Infrastructure.Common;

public static class JsonSettings
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}