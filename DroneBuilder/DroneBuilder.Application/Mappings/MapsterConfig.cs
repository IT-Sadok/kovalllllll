using Mapster;

namespace DroneBuilder.Application.Mappings;

public static class MapsterConfig
{
    public static TypeAdapterConfig Configure()
    {
        var config = new TypeAdapterConfig();

        config.Scan(typeof(MapsterConfig).Assembly);

        return config;
    }
}