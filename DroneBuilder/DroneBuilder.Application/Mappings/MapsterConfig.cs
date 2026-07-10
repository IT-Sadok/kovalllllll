using Mapster;

namespace DroneBuilder.Application.Mappings;

public static class MapsterConfig
{
    public static TypeAdapterConfig Configure()
    {
        TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;

        config.Scan(typeof(MapsterConfig).Assembly);

        return config;
    }
}
