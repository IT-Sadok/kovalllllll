using Mapster;

namespace DroneBuilder.Application.Mappings;

public static class MapsterConfig
{
    public static TypeAdapterConfig Configure()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.Scan(typeof(MapsterConfig).Assembly);
        config.Scan(typeof(ProductMapping).Assembly);
        config.Scan(typeof(UserMaping).Assembly);

        return config;
    }
}