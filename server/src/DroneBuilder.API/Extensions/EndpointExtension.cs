using DroneBuilder.API.Endpoints;
using DroneBuilder.API.Features.Products;

namespace DroneBuilder.API.Extensions;

public static class EndpointExtension
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapUserEndpoints()
            .MapProductEndpoints()
            .MapPropertyEndpoints()
            .MapValueEndpoints()
            .MapImageEndpoints()
            .MapCartEndpoints()
            .MapWarehouseEndpoints()
            .MapOrderEndpoints()
            .MapPaymentEndpoints();

        return app;
    }
}
