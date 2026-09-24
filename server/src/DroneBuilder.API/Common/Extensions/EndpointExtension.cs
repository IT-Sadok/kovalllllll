using DroneBuilder.API.Features;

namespace DroneBuilder.API.Common.Extensions;

public static class EndpointExtension
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapUserEndpoints()
            .MapProductEndpoints()
            .MapImageEndpoints()
            .MapCartEndpoints()
            .MapWarehouseEndpoints()
            .MapOrderEndpoints()
            .MapPaymentEndpoints()
            .MapImportEndpoints();

        return app;
    }
}
